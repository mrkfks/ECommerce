using System.Net;
using System.Text.Json;

namespace Dashboard.Web.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);

            if (context.Response.StatusCode == 401 && !context.Response.HasStarted)
            {
                _logger.LogWarning("401 Unauthorized - Redirecting to login");
                await HandleUnauthorized(context);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Beklenmeyen hata: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleUnauthorized(HttpContext context)
    {
        context.Response.Redirect("/Auth/Login");
        return Task.CompletedTask;
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = exception switch
        {
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Yetkisiz erişim"),
            InvalidOperationException => (HttpStatusCode.BadRequest, "Geçersiz işlem"),
            KeyNotFoundException => (HttpStatusCode.NotFound, "Kayıt bulunamadı"),
            ArgumentException => (HttpStatusCode.BadRequest, exception.Message),
            _ => (HttpStatusCode.InternalServerError, "Sunucu hatası. Lütfen daha sonra tekrar deneyiniz.")
        };

        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            success = false,
            message,
            timestamp = DateTime.UtcNow,
            details = _env.IsDevelopment() ? exception.ToString() : null
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}

public static class GlobalExceptionHandlerExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}
