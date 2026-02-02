using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.DTOs;
using Dashboard.Web.Services;

namespace Dashboard.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthApiService _authService;

        public AuthController(AuthApiService authService)
        {
            _authService = authService;
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

            var authResponse = await _authService.LoginAsync(loginDto);

            if (authResponse != null)
            {
                _authService.SetAuthCookies(authResponse);
                return RedirectToAction("Index", "Home");
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

            var authResponse = await _authService.RegisterAsync(registerDto);

            if (authResponse != null)
            {
                TempData["Success"] = "Kaydınız alınmıştır. Süper admin onayından sonra giriş yapabileceksiniz.";
                return RedirectToAction("Login");
            }

            ModelState.AddModelError("", "Kayıt işlemi başarısız. Bu email adresi zaten kullanılıyor olabilir.");
            return View(registerDto);
        }

        // GET: Profil sayfası
        [Authorize]
        [HttpGet]
        public IActionResult Profile()
        {
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
            HttpContext.Response.Cookies.Delete("RefreshToken");
            return RedirectToAction("Login");
        }

        // Token'ı cookie'den al ve JSON olarak dön
        [Authorize]
        [HttpGet]
        public IActionResult GetToken()
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
            {
                return Json(new { success = false, message = "Token bulunamadı" });
            }
            return Json(new { success = true, token = token });
        }
    }
}
