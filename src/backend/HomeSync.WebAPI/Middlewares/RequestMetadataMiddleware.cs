using HomeSync.WebAPI.Helpers;

namespace HomeSync.WebAPI.Middlewares;

public sealed class RequestMetadataMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var clientIp = ClientIpResolver.GetNormalizedClientIp(context);
        if (!string.IsNullOrWhiteSpace(clientIp))
        {
            context.Items["ClientIp"] = clientIp;
        }

        await next(context);
    }
}
