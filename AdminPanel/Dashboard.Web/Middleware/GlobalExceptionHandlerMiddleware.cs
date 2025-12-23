namespace Dashboard.Web.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);

            // Handle 401 Unauthorized
            if (context.Response.StatusCode == 401)
            {
                _logger.LogWarning("401 Unauthorized response. Attempting token refresh...");
                await HandleUnauthorized(context);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleUnauthorized(HttpContext context)
    {
        // Token expired, try to refresh
        if (context.Request.Cookies.TryGetValue("RefreshToken", out var refreshToken))
        {
            context.Items["NeedsTokenRefresh"] = new { refreshToken };
        }
        else
        {
            // No refresh token, redirect to login
            context.Response.Redirect("/Auth/Login");
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            success = false,
            message = exception.Message,
            timestamp = DateTime.UtcNow
        };

        // Set appropriate status code based on exception type
        if (exception is UnauthorizedAccessException)
        {
            context.Response.StatusCode = 401;
            response = new
            {
                success = false,
                message = "Yetkisiz erişim",
                timestamp = DateTime.UtcNow
            };
        }
        else if (exception is InvalidOperationException)
        {
            context.Response.StatusCode = 400;
        }
        else
        {
            context.Response.StatusCode = 500;
            response = new
            {
                success = false,
                message = "Sunucu hatası",
                timestamp = DateTime.UtcNow
            };
        }

        return context.Response.WriteAsJsonAsync(response);
    }
}

public static class GlobalExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}
