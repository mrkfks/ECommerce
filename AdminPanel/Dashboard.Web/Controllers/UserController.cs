using Dashboard.Web.Services;
using ECommerce.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dashboard.Web.Controllers
{
    [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
    public class UserController : Controller
    {
        private readonly UserApiService _userService;
        private readonly CompanyApiService _companyService;
        private readonly ILogger<UserController> _logger;

        public UserController(
            UserApiService userService,
            CompanyApiService companyService,
            ILogger<UserController> logger)
        {
            _userService = userService;
            _companyService = companyService;
            _logger = logger;
        }

        // =====================================================
        // PROFILE (Kullanıcı profili)
        // Profil Görüntüleme ve Düzenleme - Giriş yapmış kullanıcılar erişebilir
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            _logger.LogInformation("[UserController.Profile] Request by {User}. Roles: {Roles}", User?.Identity?.Name, string.Join(',', User?.Claims.Where(c => c.Type == "role").Select(c => c.Value) ?? Enumerable.Empty<string>()));

            var tokenExists = HttpContext.Request.Cookies.ContainsKey("AuthToken");
            _logger.LogInformation("[UserController.Profile] AuthToken cookie present: {Present}", tokenExists);

            var user = await _userService.GetProfileAsync();
            if (user == null)
            {
                _logger.LogWarning("[UserController.Profile] UserApiService returned null for profile.");
                // Redirect to login if user is unauthenticated, otherwise show friendly message
                if (!User?.Identity?.IsAuthenticated ?? true)
                    return RedirectToAction("Login", "Auth");

                TempData["Error"] = "Profil bilgileri yüklenemedi. Lütfen tekrar giriş yapın.";
                return RedirectToAction("Index", "Home");
            }

            return View(user);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Profile(UserDto user)
        {
            if (!ModelState.IsValid)
                return View(user);

            var profileDto = new UserProfileUpdateDto
            {
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName
            };

            var success = await _userService.UpdateProfileAsync(profileDto);

            if (success)
            {
                TempData["Success"] = "Profil bilgileriniz başarıyla güncellendi";
                return RedirectToAction(nameof(Profile));
            }

            ModelState.AddModelError("", "Profil güncellenirken hata oluştu.");
            return View(user);
        }
    }
}
