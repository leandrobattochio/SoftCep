using System.Threading.RateLimiting;

namespace SoftCep.Api.Core;

public static class RateLimitConfiguration
{
    public static void AddRateLimitConfiguration(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy("PerIp20Rps", httpContext =>
            {
                var ip = httpContext.Connection.RemoteIpAddress?.ToString();
                if (string.IsNullOrEmpty(ip) && httpContext.Request.Headers.TryGetValue("X-Test-IP", out var hdr))
                {
                    ip = hdr.ToString();
                }

                ip ??= "unknown";
                return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = Consts.MaxRequestPerIp,
                    Window = Consts.RequestWindow,
                    QueueLimit = 0,
                    AutoReplenishment = true
                });
            });
        });
    }
}