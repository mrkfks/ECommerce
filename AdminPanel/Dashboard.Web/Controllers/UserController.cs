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

        public UserController(
            UserApiService userService,
            CompanyApiService companyService)
        {
            _userService = userService;
            _companyService = companyService;
        }

        // =====================================================
        // PROFILE (Kullanıcı profili)
        // Profil Görüntüleme ve Düzenleme - Giriş yapmış kullanıcılar erişebilir
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userService.GetProfileAsync();
            if (user == null)
                return NotFound();

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
