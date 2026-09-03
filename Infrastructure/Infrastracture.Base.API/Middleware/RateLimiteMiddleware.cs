using Infrastracture.Base.API.Middleware.YourProject.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Infrastracture.Base.API.Middleware
{


    namespace YourProject.RateLimiting
    {
        public class RateLimitMiddleware
        {
            private readonly RequestDelegate _next;
            private readonly ILogger<RateLimitMiddleware> _logger;

            public RateLimitMiddleware(RequestDelegate next, ILogger<RateLimitMiddleware> logger)
            {
                _next = next;
                _logger = logger;
            }

            public async Task InvokeAsync(HttpContext context)
            {
                // Register callback to set headers before response starts
                context.Response.OnStarting(() =>
                {
                    if (context.Request.Path.StartsWithSegments("/api"))
                    {
                        // Use TryAdd to avoid conflicts with other middleware
                        context.Response.Headers.TryAdd("X-Rate-Limit", "1000");
                        context.Response.Headers.TryAdd("X-Rate-Limit-Remaining", "999");

                        // Add Retry-After if this will be a 429 response
                        if (context.Response.StatusCode == StatusCodes.Status429TooManyRequests)
                        {
                            context.Response.Headers.TryAdd("Retry-After", "60");
                        }
                    }
                    return Task.CompletedTask;
                });

                var startTime = DateTime.UtcNow;
                await _next(context);

                // Logging only - no header modifications
                if (context.Response.StatusCode == StatusCodes.Status429TooManyRequests)
                {
                    var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    var endpoint = context.Request.Path;

                    _logger.LogWarning("Rate limit exceeded - Client: {ClientIP}, Endpoint: {Endpoint}, Method: {Method}",
                        clientIp, endpoint, context.Request.Method);
                }
            }
        }
    }

    public static class RateLimitMiddlewareExtensions
    {
        public static IApplicationBuilder UseRateLimitHeaders(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RateLimitMiddleware>();
        }
    }
}


