using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.JsonWebTokens;

namespace HomeSync.WebAPI.Middlewares;

public sealed class RateLimitCounter
{
    public int Count;
}

public sealed class RateLimitingMiddleware(RequestDelegate next, IMemoryCache cache, ILogger<RateLimitingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // SignalR request
        if (context.Request.Path.StartsWithSegments("/logHub"))
        {
            await next(context);
            return;
        }

        var userEmail = context.User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
        var clientIp = context.Items["ClientIp"]?.ToString();

        string cacheKey;
        int limit;
        string policyName;

        if (!string.IsNullOrEmpty(userEmail)) // Email bazlı
        {
            cacheKey = $"rl_user_{userEmail}";
            limit = 200;
            policyName = "UserBased";
        }
        else if (!string.IsNullOrEmpty(clientIp)) // IP bazlı
        {
            cacheKey = $"rl_ip_{clientIp}";
            limit = 200;
            policyName = "IpBased";
        }
        else
        {
            cacheKey = "rl_global_pool";
            limit = 2000;
            policyName = "GlobalPool";
        }

        var counter = cache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            return new RateLimitCounter();
        })!;

        int currentCount = Interlocked.Increment(ref counter.Count);

        if (currentCount > limit)
        {
            logger.LogWarning("Rate limit exceeded for {Policy}: {Key}", policyName, cacheKey);

            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.ContentType = "text/plain; charset=utf-8";
            await context.Response.WriteAsync("LIMIT_REACHED");
            return;
        }

        context.Response.Headers["X-Rate-Limit-Policy"] = policyName;
        context.Response.Headers["X-Rate-Limit-Remaining"] = Math.Max(0, limit - currentCount).ToString();

        await next(context);
    }
}
