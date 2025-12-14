using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using ECommerce.Application.DTOs;

namespace Dashboard.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // GET: Login formu
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: Login işlemi
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return View(loginDto);

            var isHttps = HttpContext.Request.IsHttps;

            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var response = await client.PostAsJsonAsync("api/auth/login", loginDto);

            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

                if (authResponse != null && !string.IsNullOrEmpty(authResponse.AccessToken))
                {
                    // HttpOnly cookie - server-side operations için
                    HttpContext.Response.Cookies.Append("AuthToken", authResponse.AccessToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = isHttps,
                        SameSite = SameSiteMode.Strict,
                        Expires = authResponse.ExpiresAt
                    });

                    // Regular cookie - JavaScript'ten erişim için (DEVELOPMENT ONLY)
                    HttpContext.Response.Cookies.Append("JwtToken", authResponse.AccessToken, new CookieOptions
                    {
                        HttpOnly = false,
                        Secure = isHttps,
                        SameSite = SameSiteMode.Lax,
                        Expires = authResponse.ExpiresAt
                    });

                    return RedirectToAction("Index", "Home");
                }
            }

            ModelState.AddModelError("", "Email/Kullanıcı adı veya şifre yanlış.");
            return View(loginDto);
        }

        // GET: Register formu
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: Register işlemi
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
                return View(registerDto);

            var isHttps = HttpContext.Request.IsHttps;

            // Admin panelde company seçimi yok; demo/tek tenant senaryosunda varsayılan company.
            if (registerDto.CompanyId <= 0)
                registerDto.CompanyId = 1;

            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var response = await client.PostAsJsonAsync("api/auth/register", registerDto);

            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

                if (authResponse != null && !string.IsNullOrEmpty(authResponse.AccessToken))
                {
                    // HttpOnly cookie - server-side operations için
                    HttpContext.Response.Cookies.Append("AuthToken", authResponse.AccessToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = isHttps,
                        SameSite = SameSiteMode.Strict,
                        Expires = authResponse.ExpiresAt
                    });

                    // Regular cookie - JavaScript'ten erişim için (DEVELOPMENT ONLY)
                    HttpContext.Response.Cookies.Append("JwtToken", authResponse.AccessToken, new CookieOptions
                    {
                        HttpOnly = false,
                        Secure = isHttps,
                        SameSite = SameSiteMode.Lax,
                        Expires = authResponse.ExpiresAt
                    });

                    return RedirectToAction("Index", "Home");
                }
            }

            ModelState.AddModelError("", "Kayıt işlemi başarısız. Lütfen tekrar deneyin.");
            return View(registerDto);
        }

        // GET: Profil sayfası
        [Authorize]
        [HttpGet]
        public IActionResult Profile()
        {
            // TODO: Login sistemi aktif olunca token kontrolü eklenecek
            // var token = HttpContext.Request.Cookies["AuthToken"];
            // if (string.IsNullOrEmpty(token))
            // {
            //     return RedirectToAction("Login");
            // }

            // Demo profil bilgileri
            var userDto = new UserDto
            {
                Id = 1,
                Username = "admin",
                Email = "admin@ecommerce.com",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                Roles = new List<string> { "Admin", "Manager" },
                CompanyName = "ECommerce Company",
                CompanyId = 1
            };

            return View(userDto);
        }

        // Çıkış
        [AllowAnonymous]
        public IActionResult Logout()
        {
            HttpContext.Response.Cookies.Delete("AuthToken");
            HttpContext.Response.Cookies.Delete("JwtToken");
            return RedirectToAction("Login");
        }

        // GET: Token'ı JavaScript'e döndür (DEVELOPMENT ONLY)
        [AllowAnonymous]
        [HttpGet]
        public IActionResult GetToken()
        {
            var token = HttpContext.Request.Cookies["JwtToken"];
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new { message = "Token bulunamadı" });
            }
            return Ok(new { token });
        }
    }
}
