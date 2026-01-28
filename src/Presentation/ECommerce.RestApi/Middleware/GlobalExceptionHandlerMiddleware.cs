using System.Net;
using System.Text.Json;
using ECommerce.Application.Exceptions;
using FluentValidation;

namespace ECommerce.RestApi.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IHostEnvironment env)
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
        }
        catch (Exception ex)
        {
            var endpoint = context.GetEndpoint()?.DisplayName;
            var user = context.User.Identity?.Name ?? "Anonymous";
            var userId = context.User.FindFirst("id")?.Value;

            _logger.LogError(ex, 
                "An unhandled exception occurred. Endpoint: {Endpoint}, User: {User} ({UserId}), Message: {Message}", 
                endpoint, user, userId, ex.Message);
                
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = new ECommerce.Application.DTOs.ApiResponseDto<object>
        {
            Success = false
        };

        switch (exception)
        {
            case Application.Exceptions.ValidationException validationEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = "Validation Error";
                response.Data = validationEx.Errors;
                break;

            case AppException appEx when appEx.StatusCode.HasValue:
                context.Response.StatusCode = appEx.StatusCode.Value;
                response.Message = appEx.Message;
                break;

            case UnauthorizedAccessException:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.Message = "You are not authorized to access this resource.";
                break;

            case KeyNotFoundException:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Message = exception.Message;
                break;

            case InvalidOperationException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = exception.Message;
                break;

            case ArgumentException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = exception.Message;
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Message = _env.IsDevelopment() ? exception.Message : "An error occurred while processing your request.";
                
                if (_env.IsDevelopment())
                {
                    response.Data = new { StackTrace = exception.StackTrace };
                }
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}
