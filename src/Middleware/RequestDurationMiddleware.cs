using System.Diagnostics;

namespace BookingHub.Service.Middleware;

public class RequestDurationMiddleware(RequestDelegate next, ILogger<RequestDurationMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();
            logger.LogInformation(
                "{Method} {Path} responded {StatusCode} in {ElapsedMilliseconds}ms",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
    }
}

public static class RequestDurationMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestDuration(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestDurationMiddleware>();
    }
}
