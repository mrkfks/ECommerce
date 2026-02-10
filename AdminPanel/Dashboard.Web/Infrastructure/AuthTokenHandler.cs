using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Dashboard.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Dashboard.Web.Infrastructure
{
    public class AuthTokenHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AuthTokenHandler> _logger;
        private const string RefreshAttemptedHeader = "X-Token-Refresh-Attempted";

        public AuthTokenHandler(IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider, ILogger<AuthTokenHandler> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var context = _httpContextAccessor.HttpContext;
            var token = context?.Request.Cookies["AuthToken"];

            _logger.LogWarning("[AuthTokenHandler] Request: {Method} {Url}", request.Method, request.RequestUri);
            _logger.LogWarning("[AuthTokenHandler] HttpContext null: {IsNull}, Token present: {HasToken}",
                context == null, !string.IsNullOrWhiteSpace(token));

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // JWT'den Company ID'yi al ve header'a ekle
                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(token);
                    var companyId = jwtToken.Claims.FirstOrDefault(c => c.Type == "CompanyId")?.Value;

                    if (!string.IsNullOrEmpty(companyId))
                    {
                        request.Headers.Add("X-Company-Id", companyId);
                    }
                }
                catch
                {
                    // JWT parse hatası - devam et
                }
            }

            var response = await base.SendAsync(request, cancellationToken);

            // Token expired (401) - refresh token'ı kullanarak yeni token al
            // Sonsuz döngüyü önlemek için refresh zaten denenmişse tekrar deneme
            if (response.StatusCode == HttpStatusCode.Unauthorized &&
                !request.Headers.Contains(RefreshAttemptedHeader))
            {
                var refreshToken = context?.Request.Cookies["RefreshToken"];
                if (!string.IsNullOrWhiteSpace(refreshToken))
                {
                    var authService = _serviceProvider.GetService<AuthApiService>();
                    if (authService != null)
                    {
                        var newAuthResponse = await authService.RefreshTokenAsync(refreshToken);
                        if (newAuthResponse != null)
                        {
                            // Yeni token'ları ayarla
                            authService.SetAuthCookies(newAuthResponse);

                            // Yeni request oluştur (orijinali tekrar kullanılamaz)
                            var newRequest = await CloneHttpRequestMessageAsync(request);
                            newRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", newAuthResponse.AccessToken);
                            newRequest.Headers.Add(RefreshAttemptedHeader, "true");

                            return await base.SendAsync(newRequest, cancellationToken);
                        }
                    }
                }
            }

            return response;
        }

        private static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri);

            // Content'i kopyala
            if (request.Content != null)
            {
                var ms = new MemoryStream();
                await request.Content.CopyToAsync(ms);
                ms.Position = 0;
                clone.Content = new StreamContent(ms);

                // Content headers'ı kopyala
                foreach (var header in request.Content.Headers)
                {
                    clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            // Headers'ı kopyala (Authorization hariç, onu sonra ayarlayacağız)
            foreach (var header in request.Headers)
            {
                if (header.Key != "Authorization")
                {
                    clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            // Options'ı kopyala
            foreach (var option in request.Options)
            {
                clone.Options.TryAdd(option.Key, option.Value);
            }

            clone.Version = request.Version;

            return clone;
        }
    }
}