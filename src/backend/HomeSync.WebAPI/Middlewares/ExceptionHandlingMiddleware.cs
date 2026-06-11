using System.Net;

namespace HomeSync.WebAPI.Middlewares;

public class ExceptionHandlingMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context, ILogger<ExceptionHandlingMiddleware> logger)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            if (ex.GetType() == typeof(InvalidOperationException) && ex.Message.Equals("HARDWARE_ERROR"))
            {
                logger.LogError(ex,
                    "Kritik donanımsal sensör arızası! Mesaj işlenemedi. TraceId: {TraceId} | Method: {Method} | Path: {Path}{QueryString}",
                    context.TraceIdentifier,
                    context.Request.Method,
                    context.Request.Path,
                    context.Request.QueryString.HasValue ? context.Request.QueryString.Value : string.Empty);

                await HandleExceptionAsync(context, ex.Message);
            }
            else
            {
                logger.LogError(ex,
                    "An unhandled exception occurred. TraceId: {TraceId} | Method: {Method} | Path: {Path}{QueryString}",
                    context.TraceIdentifier,
                    context.Request.Method,
                    context.Request.Path,
                    context.Request.QueryString.HasValue ? context.Request.QueryString.Value : string.Empty);
                await HandleExceptionAsync(context);
            }
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, string message = "Beklenmeyen bir hata oluştu.")
    {
        HttpStatusCode status = HttpStatusCode.InternalServerError;

        context.Response.ContentType = "text/plain; charset=utf-8";
        context.Response.StatusCode = (int)status;

        await context.Response.WriteAsync(message);
    }
}
