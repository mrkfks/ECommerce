using ECommerce.Application.DTOs;

namespace Dashboard.Web.Services
{
    public class AuthApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Auth/login", loginDto);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<AuthResponseDto>();
                }
                
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Auth/register", registerDto);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<AuthResponseDto>();
                }
                
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<UserDto?> GetProfileAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<UserDto>("api/Auth/profile");
            }
            catch
            {
                return null;
            }
        }

        public async Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                var request = new RefreshTokenRequest { RefreshToken = refreshToken };
                var response = await _httpClient.PostAsJsonAsync("api/Auth/refresh", request);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<AuthResponseDto>();
                }
                
                return null;
            }
            catch
            {
                return null;
            }
        }

        public void SetAuthCookies(AuthResponseDto authResponse)
        {
            if (authResponse == null)
                return;

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return;

            var isHttps = httpContext.Request.IsHttps;

            // HttpOnly cookie - server-side operations için
            httpContext.Response.Cookies.Append("AuthToken", authResponse.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = isHttps,
                SameSite = SameSiteMode.Strict,
                Expires = authResponse.ExpiresAt
            });

            // Refresh token - server-side operations için
            if (!string.IsNullOrEmpty(authResponse.RefreshToken))
            {
                httpContext.Response.Cookies.Append("RefreshToken", authResponse.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = isHttps,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(7) // Refresh token 7 gün geçerli
                });
            }

            // Regular cookie - JavaScript'ten erişim için (DEVELOPMENT ONLY)
            httpContext.Response.Cookies.Append("JwtToken", authResponse.AccessToken, new CookieOptions
            {
                HttpOnly = false,
                Secure = isHttps,
                SameSite = SameSiteMode.Lax,
                Expires = authResponse.ExpiresAt
            });
        }
    }
}
