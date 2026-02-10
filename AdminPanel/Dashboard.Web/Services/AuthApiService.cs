using System.Text.Json;
using ECommerce.Application.DTOs;
using ECommerce.Application.Responses;
using Microsoft.Extensions.Logging;

namespace Dashboard.Web.Services
{
    public class AuthApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuthApiService> _logger;

        public AuthApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<AuthApiService> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Auth/login", loginDto);

                if (response.IsSuccessStatusCode)
                {
                    // API returns wrapped response: { success, data: AuthResponseDto, message }
                    var wrappedResponse = await response.Content.ReadFromJsonAsync<ECommerce.Application.Responses.ApiResponse<AuthResponseDto>>();
                    var result = wrappedResponse?.Data;

                    _logger.LogInformation("Login successful for: {Username}", result?.Username);
                    return result;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Login failed: {StatusCode} - {Content}", response.StatusCode, errorContent);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during login");
                return null;
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/companies/register", registerDto);

                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }

                var content = await response.Content.ReadAsStringAsync();

                try
                {
                    using var doc = JsonDocument.Parse(content);
                    if (doc.RootElement.ValueKind == JsonValueKind.Object)
                    {
                        if (doc.RootElement.TryGetProperty("message", out var msgProp) && msgProp.ValueKind == JsonValueKind.String)
                        {
                            var msg = msgProp.GetString();
                            if (!string.IsNullOrWhiteSpace(msg))
                                return (false, msg);
                        }

                        if (doc.RootElement.TryGetProperty("Message", out var msgProp2) && msgProp2.ValueKind == JsonValueKind.String)
                        {
                            var msg = msgProp2.GetString();
                            if (!string.IsNullOrWhiteSpace(msg))
                                return (false, msg);
                        }
                    }
                }
                catch { }

                return (false, content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during register");
                return (false, ex.Message);
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
