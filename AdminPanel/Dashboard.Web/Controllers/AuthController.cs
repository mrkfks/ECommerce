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
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: Login işlemi
        [HttpPost]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return View(loginDto);

            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var response = await client.PostAsJsonAsync("auth/login", loginDto);

            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

                if (authResponse != null && !string.IsNullOrEmpty(authResponse.AccessToken))
                {
                    HttpContext.Response.Cookies.Append("AuthToken", authResponse.AccessToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = authResponse.ExpiresAt
                    });

                    return RedirectToAction("Index", "Home");
                }
            }

            ModelState.AddModelError("", "Giriş başarısız.");
            return View(loginDto);
        }

        // Çıkış
        public IActionResult Logout()
        {
            HttpContext.Response.Cookies.Delete("AuthToken");
            return RedirectToAction("Login");
        }
    }
}
