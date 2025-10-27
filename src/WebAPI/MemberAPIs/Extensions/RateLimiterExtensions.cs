using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using MemberAPIs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MemberAPIs.Constants;

namespace MemberAPIs.Extensions
{
    public static class RateLimiterExtensions
    {
        private static string GetPartitionKey(HttpContext httpContext)
        {
            var userIdentity = httpContext.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(userIdentity))
            {
                return $"RateLimitByGlobalKey_User_{userIdentity}";
            }

            var ipAddress = httpContext.Connection?.RemoteIpAddress?.ToString();
            if (!string.IsNullOrEmpty(ipAddress))
            {
                return $"RateLimitByGlobalKey_IP_{ipAddress}";
            }

            return "RateLimitByGlobalKey";
        }

        public static IServiceCollection AddCustomRateLimiter(this IServiceCollection services, IConfiguration configuration)
        {
            var rateLimiterOptions = new MemberAPIs.Models.RateLimiterOptions();
            configuration.GetSection("RateLimitConfigs:RateLimitByGlobal").Bind(rateLimiterOptions);

            services.AddRateLimiter(options =>
            {
                options.OnRejected = async (context, token) =>
                {
                    var httpContext = context.HttpContext;
                    var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterValue)
                        ? retryAfterValue.TotalSeconds
                        : 60;

                    httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    httpContext.Response.Headers.Add("Retry-After", retryAfter.ToString());
                    await httpContext.Response.WriteAsync(ApiMessagesConstants.RateLimitExceeded, token);
                };

                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                {
                    var partitionKey = GetPartitionKey(httpContext);
                    return RateLimitPartition.GetSlidingWindowLimiter(
                        partitionKey: partitionKey,
                        factory: _ => new SlidingWindowRateLimiterOptions
                        {
                            AutoReplenishment = rateLimiterOptions.AutoReplenishment,
                            PermitLimit = rateLimiterOptions.PermitLimit,
                            Window = rateLimiterOptions.GetWindowTimeSpan(),
                            SegmentsPerWindow = rateLimiterOptions.SegmentsPerWindow,
                        });
                });
            });

            return services;
        }

        public static IApplicationBuilder UseCustomRateLimiter(this IApplicationBuilder app, IConfiguration configuration)
        {
            app.UseWhen(context =>
                context.Request.Path.Value.Trim() != "/" &&
                !context.Request.Path.Value.ToLower().Trim().Contains("/health"), applicationBuilder =>
            {
                applicationBuilder.UseRateLimiter();
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health").AllowAnonymous().DisableRateLimiting();
            });

            return app;
        }
    }
}