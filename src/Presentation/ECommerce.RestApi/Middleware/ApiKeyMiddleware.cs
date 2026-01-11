using System.Threading.Tasks;
using ECommerce.RestApi.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ECommerce.RestApi.Middleware
{
    /// <summary>
    /// API Key -> CompanyId eşlemesi yapar. Geçerli anahtar bulunursa HttpContext.Items["CompanyId"] set edilir.
    /// Eğer geçersiz bir anahtar sağlanırsa 401 döner, hiç anahtar yoksa istek akmaya devam eder.
    /// </summary>
    public class ApiKeyMiddleware
    {
        private const string HeaderName = "X-Api-Key";
        private readonly RequestDelegate _next;
        private readonly ApiKeyOptions _options;
        private readonly ILogger<ApiKeyMiddleware> _logger;

        public ApiKeyMiddleware(RequestDelegate next, IOptions<ApiKeyOptions> options, ILogger<ApiKeyMiddleware> logger)
        {
            _next = next;
            _options = options.Value;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue(HeaderName, out var keyValues))
            {
                var apiKey = keyValues.ToString();
                if (_options.Keys.TryGetValue(apiKey, out var companyId))
                {
                    _logger.LogInformation("Valid API key received. CompanyId set to {CompanyId}", companyId);
                    context.Items["CompanyId"] = companyId;
                }
                else
                {
                    _logger.LogWarning("Invalid API key: {ApiKey}", apiKey);
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Invalid API key.");
                    return;
                }
            }

            await _next(context);
        }
    }
}
