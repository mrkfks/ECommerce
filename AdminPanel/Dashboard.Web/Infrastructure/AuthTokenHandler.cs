using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Dashboard.Web.Services;

namespace Dashboard.Web.Infrastructure
{
    public class AuthTokenHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IServiceProvider _serviceProvider;

        public AuthTokenHandler(IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _serviceProvider = serviceProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var context = _httpContextAccessor.HttpContext;
            var token = context?.Request.Cookies["AuthToken"];

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await base.SendAsync(request, cancellationToken);

            // Token expired (401) - refresh token'ı kullanarak yeni token al
            if (response.StatusCode == HttpStatusCode.Unauthorized)
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

                            // Request'i yeni token ile tekrar dene
                            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", newAuthResponse.AccessToken);
                            return await base.SendAsync(request, cancellationToken);
                        }
                    }
                }
            }

            return response;
        }
    }
}