using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.RateLimiting;

namespace Infrastracture.Base.API.Extentions
{
    public static class RateLimitConfiguration
    {
        public static IServiceCollection ConfigureRateLimiting(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    {
                        await context.HttpContext.Response.WriteAsync(
                            $$"""
                            {
                                "error": "Too many requests",
                                "message": "Please try again after {{retryAfter.TotalSeconds}} seconds.",
                                "retryAfter": {{retryAfter.TotalSeconds}}
                            }
                            """,
                            cancellationToken: token);
                    }
                    else
                    {
                        await context.HttpContext.Response.WriteAsync(
                            """
                            {
                                "error": "Too many requests",
                                "message": "Please try again later."
                            }
                            """,
                            cancellationToken: token);
                    }
                };

                // Global rate limiter - applies public-api limits to all endpoints
                // Endpoint-specific [EnableRateLimiting] policies stack on top of this
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                {
                    var clientId = GetClientIdentifier(context);
                    return RateLimitPartition.GetFixedWindowLimiter($"global_{clientId}", _ =>
                        new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 30,
                            Window = TimeSpan.FromMinutes(1),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 3
                        });
                });

                // Fixed Window Policy - General API limits
                options.AddFixedWindowLimiter(RateLimitPolicies.FixedWindow, fixedOptions =>
                {
                    fixedOptions.PermitLimit = 100;
                    fixedOptions.Window = TimeSpan.FromMinutes(1);
                    fixedOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    fixedOptions.QueueLimit = 10;
                });

                // Sliding Window Policy - For burst protection
                options.AddSlidingWindowLimiter(RateLimitPolicies.SlidingWindow, slidingOptions =>
                {
                    slidingOptions.PermitLimit = 50;
                    slidingOptions.Window = TimeSpan.FromSeconds(30);
                    slidingOptions.SegmentsPerWindow = 3;
                    slidingOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    slidingOptions.QueueLimit = 5;
                });

                // Token Bucket Policy - For sustained traffic
                options.AddTokenBucketLimiter(RateLimitPolicies.TokenBucket, tokenOptions =>
                {
                    tokenOptions.TokenLimit = 100;
                    tokenOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    tokenOptions.QueueLimit = 5;
                    tokenOptions.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
                    tokenOptions.TokensPerPeriod = 20;
                    tokenOptions.AutoReplenishment = true;
                });

                // Concurrency Limit - For resource-intensive operations
                options.AddConcurrencyLimiter(RateLimitPolicies.Concurrency, concurrencyOptions =>
                {
                    concurrencyOptions.PermitLimit = 10;
                    concurrencyOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    concurrencyOptions.QueueLimit = 5;
                });

                // Strict Policy - For sensitive operations
                options.AddFixedWindowLimiter(RateLimitPolicies.Strict, strictOptions =>
                {
                    strictOptions.PermitLimit = 10;
                    strictOptions.Window = TimeSpan.FromMinutes(1);
                    strictOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    strictOptions.QueueLimit = 2;
                });

                // Public API Policy - For external endpoints
                options.AddFixedWindowLimiter(RateLimitPolicies.PublicAPI, publicOptions =>
                {
                    publicOptions.PermitLimit = 30;
                    publicOptions.Window = TimeSpan.FromMinutes(1);
                    publicOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    publicOptions.QueueLimit = 3;
                });

                // Authenticated User Policy - Higher limits for authenticated users
                options.AddFixedWindowLimiter(RateLimitPolicies.AuthenticatedUser, authOptions =>
                {
                    authOptions.PermitLimit = 200;
                    authOptions.Window = TimeSpan.FromMinutes(1);
                    authOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    authOptions.QueueLimit = 10;
                });

                // OTP Rate Limiting - 1 request per 2 minutes per client
                options.AddPolicy(RateLimitPolicies.OtpStrict, context =>
                {
                    var clientId = GetClientIdentifier(context);
                    return RateLimitPartition.GetFixedWindowLimiter($"otp_{clientId}", _ =>
                        new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 1,
                            Window = TimeSpan.FromMinutes(2),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        });
                });

                // Client-based Rate Limiting
                options.AddPolicy(RateLimitPolicies.PerClient, context =>
                {
                    var clientId = GetClientIdentifier(context);
                    return RateLimitPartition.GetFixedWindowLimiter(clientId, _ =>
                        new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 50,
                            Window = TimeSpan.FromMinutes(1),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 5
                        });
                });
            });

            return services;
        }

        private static string GetClientIdentifier(HttpContext context)
        {
            // Try to get client ID from various sources
            if (context.User.Identity?.IsAuthenticated == true)
            {
                return context.User.FindFirst("sub")?.Value ??
                       context.User.FindFirst("client_id")?.Value ??
                       context.User.Identity.Name ?? "authenticated";
            }

            return context.Request.Headers["X-Client-Id"].FirstOrDefault() ??
                   context.Request.Headers["X-API-Key"].FirstOrDefault() ??
                   context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
        }
    }

    public static class RateLimitPolicies
    {
        public const string FixedWindow = "fixed-window";
        public const string SlidingWindow = "sliding-window";
        public const string TokenBucket = "token-bucket";
        public const string Concurrency = "concurrency";
        public const string AuthenticatedUser = "authenticated-user";
        public const string PerClient = "per-client";
        public const string Strict = "strict";
        public const string PublicAPI = "public-api";
        public const string OtpStrict = "otp-strict";
    }
}
