using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastracture.Base.API.Middleware
{
    /// <summary>
    /// Last-resort handler for exceptions that escape a controller or handler. Logs the
    /// exception in full server-side and returns only a generic message plus a trace id,
    /// in the same JSON shape the controllers already use for errors:
    /// { "message": "...", "messageCode": "&lt;trace id&gt;" }.
    /// </summary>
    public class GlobalExceptionHandlingMiddleware
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

        public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;

            context.Response.OnStarting(() =>
            {
                context.Response.Headers.TryAdd("X-Trace-Id", traceId);
                return Task.CompletedTask;
            });

            // Scope so that any ILogger call made while handling this request carries the
            // same trace id the client is shown.
            using (_logger.BeginScope(new Dictionary<string, object> { ["TraceId"] = traceId }))
            {
                try
                {
                    await _next(context);
                }
                catch (Exception ex)
                {
                    await HandleAsync(context, ex, traceId);
                }
            }
        }

        private async Task HandleAsync(HttpContext context, Exception ex, string traceId)
        {
            var statusCode = MapStatusCode(ex);

            _logger.LogError(
                ex,
                "Unhandled exception. TraceId: {TraceId}, Method: {Method}, Path: {Path}, StatusCode: {StatusCode}",
                traceId, context.Request.Method, context.Request.Path, statusCode);

            // The response has already begun streaming; the body cannot be rewritten and
            // rewriting the headers would throw. Abort so the client sees a failed request
            // rather than a truncated body it would parse as success.
            if (context.Response.HasStarted)
            {
                _logger.LogWarning("Response already started for TraceId {TraceId}; aborting connection.", traceId);
                context.Abort();
                return;
            }

            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var body = JsonSerializer.Serialize(
                new { message = MapMessage(ex, statusCode), messageCode = traceId },
                SerializerOptions);

            await context.Response.WriteAsync(body);
        }

        private static int MapStatusCode(Exception ex) => ex switch
        {
            UnauthorizedAccessException => StatusCodes.Status403Forbidden,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            OperationCanceledException => StatusCodes.Status499ClientClosedRequest,
            TimeoutException => StatusCodes.Status504GatewayTimeout,
            NotImplementedException => StatusCodes.Status501NotImplemented,
            _ => StatusCodes.Status500InternalServerError
        };

        // Only status-derived text is returned. ex.Message is never surfaced: it routinely
        // carries DB schema, SQL, and connection details.
        private static string MapMessage(Exception ex, int statusCode) => statusCode switch
        {
            StatusCodes.Status403Forbidden => "You are not authorized to perform this action.",
            StatusCodes.Status404NotFound => "The requested resource was not found.",
            StatusCodes.Status499ClientClosedRequest => "The request was cancelled.",
            StatusCodes.Status504GatewayTimeout => "The request timed out. Please try again.",
            StatusCodes.Status501NotImplemented => "This operation is not supported.",
            _ => SafeError.UnexpectedMessage
        };
    }

    public static class GlobalExceptionHandlingMiddlewareExtensions
    {
        /// <summary>
        /// Must be registered first in the pipeline so it wraps every other middleware.
        /// </summary>
        public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
        }
    }
}
