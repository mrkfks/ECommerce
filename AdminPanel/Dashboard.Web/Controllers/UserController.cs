using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.DTOs;
using Dashboard.Web.Services;

namespace Dashboard.Web.Controllers
{
    [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
    public class UserController : Controller
    {
        private readonly UserApiService _userService;
        private readonly CompanyApiService _companyService;
        private readonly UserManagementApiService _userManagementService;
        private readonly LoginHistoryApiService _loginHistoryService;

        public UserController(
            UserApiService userService,
            CompanyApiService companyService,
            UserManagementApiService userManagementService,
            LoginHistoryApiService loginHistoryService)
        {
            _userService = userService;
            _companyService = companyService;
            _userManagementService = userManagementService;
            _loginHistoryService = loginHistoryService;
        }

        // Listeleme
        public async Task<IActionResult> Index(int? companyId = null)
        {
            var users = companyId.HasValue
                ? await _userService.GetByCompanyAsync(companyId.Value)
                : await _userService.GetAllAsync();

            // Şirket listesini ViewBag'e ekle
            ViewBag.Companies = await _companyService.GetAllAsync();
            ViewBag.SelectedCompanyId = companyId;

            return View(users);
        }

        // =====================================================
        // USER MANAGEMENT (Gelişmiş kullanıcı yönetimi)
        // =====================================================

        /// <summary>
        /// Kullanıcı yönetimi ana sayfası - filtreleme, arama, sayfalama
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Management(
            string? search = null,
            string? role = null,
            int? companyId = null,
            bool? isActive = null,
            int page = 1,
            int pageSize = 20)
        {
            var filter = new UserFilterVm
            {
                SearchTerm = search,
                Role = role,
                CompanyId = companyId,
                IsActive = isActive,
                Page = page,
                PageSize = pageSize
            };

            var pagedUsers = await _userManagementService.GetAllAsync(filter);
            var roles = await _userManagementService.GetRolesAsync();
            var summary = await _userManagementService.GetSummaryAsync();
            var companies = await _companyService.GetAllAsync();

            ViewBag.Roles = roles;
            ViewBag.Companies = companies;
            ViewBag.Summary = summary;
            ViewBag.Filter = filter;

            return View(pagedUsers);
        }

        /// <summary>
        /// Kullanıcı yönetimi özeti (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetManagementSummary()
        {
            var summary = await _userManagementService.GetSummaryAsync();
            return Json(summary);
        }

        /// <summary>
        /// Son giriş yapan kullanıcılar
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> LoginHistory(int? userId = null, int take = 50)
        {
            var logins = userId.HasValue
                ? await _loginHistoryService.GetAllAsync(userId: userId.Value, take: take)
                : await _loginHistoryService.GetAllAsync(take: take);

            ViewBag.UserId = userId;
            return View(logins);
        }

        /// <summary>
        /// Şüpheli girişler
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SuspiciousLogins()
        {
            var logins = await _loginHistoryService.GetSuspiciousAsync();
            return View(logins);
        }

        /// <summary>
        /// Giriş geçmişi özeti (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetLoginHistorySummary()
        {
            var summary = await _loginHistoryService.GetSummaryAsync();
            return Json(summary);
        }

        /// <summary>
        /// Son girişler (AJAX - Dashboard widget için)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetRecentLogins(int take = 10)
        {
            var logins = await _loginHistoryService.GetRecentAsync(take);
            return Json(logins);
        }

        /// <summary>
        /// Kullanıcı rollerini güncelle (AJAX)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateUserRoles([FromBody] UserRoleUpdateRequest request)
        {
            var success = await _userManagementService.UpdateRolesAsync(request.UserId, request.Roles);
            return Json(new { success, message = success ? "Roller güncellendi." : "Güncelleme başarısız." });
        }

        /// <summary>
        /// Kullanıcı aktivasyon durumunu değiştir (AJAX)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ToggleUserActivation([FromBody] UserActivationRequest request)
        {
            var success = await _userManagementService.UpdateActivationAsync(request.UserId, request.IsActive);
            return Json(new { success, message = success ? (request.IsActive ? "Kullanıcı aktifleştirildi." : "Kullanıcı devre dışı bırakıldı.") : "İşlem başarısız." });
        }

        /// <summary>
        /// Şüpheli işareti kaldır (AJAX)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ClearSuspicion([FromBody] int loginHistoryId)
        {
            var success = await _loginHistoryService.ClearSuspicionAsync(loginHistoryId);
            return Json(new { success, message = success ? "Şüpheli işareti kaldırıldı." : "İşlem başarısız." });
        }

        // Yeni kullanıcı ekleme - GET
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var roles = await _userService.GetRolesAsync();
            ViewBag.AllRoles = roles ?? new List<string>();

            // SuperAdmin için şirket listesi
            ViewBag.Companies = await _companyService.GetAllAsync();

            return View();
        }

        // Yeni kullanıcı ekleme - POST
        [HttpPost]
        public async Task<IActionResult> Create(UserCreateDto dto, List<string> SelectedRoles)
        {
            if (!ModelState.IsValid)
            {
                var roles = await _userService.GetRolesAsync();
                ViewBag.AllRoles = roles ?? new List<string>();
                ViewBag.Companies = await _companyService.GetAllAsync();
                return View(dto);
            }

            // İlk seçilen rolü ata
            if (SelectedRoles != null && SelectedRoles.Any())
            {
                dto.RoleName = SelectedRoles.First();
            }

            var success = await _userService.CreateAsync(dto);

            if (success)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Kullanıcı oluşturulurken hata oluştu.");
            var roleList = await _userService.GetRolesAsync();
            ViewBag.AllRoles = roleList ?? new List<string>();
            ViewBag.Companies = await _companyService.GetAllAsync();
            return View(dto);
        }

        // Detay
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userService.GetByIdAsync(id);

            if (user == null)
                return NotFound();

            return View(user);
        }

        // Rol atama/düzenleme
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userService.GetByIdAsync(id);

            if (user == null)
                return NotFound();

            var roles = await _userService.GetRolesAsync();
            ViewBag.AllRoles = roles ?? new List<string>();

            // SuperAdmin için şirket listesi
            ViewBag.Companies = await _companyService.GetAllAsync();

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UserDto user, List<string> SelectedRoles)
        {
            if (!ModelState.IsValid)
            {
                var roles = await _userService.GetRolesAsync();
                ViewBag.AllRoles = roles ?? new List<string>();
                ViewBag.Companies = await _companyService.GetAllAsync();
                return View(user);
            }

            // DTO'ya dönüştür
            var updateDto = new UserUpdateDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                CompanyId = user.CompanyId,
                IsActive = user.IsActive,
                Roles = SelectedRoles ?? new List<string>()
            };

            var success = await _userService.UpdateAsync(user.Id, updateDto);

            if (success)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Kullanıcı güncellenirken hata oluştu.");
            var roleList = await _userService.GetRolesAsync();
            ViewBag.AllRoles = roleList ?? new List<string>();
            ViewBag.Companies = await _companyService.GetAllAsync();
            return View(user);
        }

        // Silme
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userService.GetByIdAsync(id);

            if (user == null)
                return NotFound();

            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var success = await _userService.DeleteAsync(id);

            if (success)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Kullanıcı silinirken hata oluştu.");
            var user = await _userService.GetByIdAsync(id);
            return View(user);
        }

        // Profil Görüntüleme ve Düzenleme - Herkes Erişebilir
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userService.GetProfileAsync();
            if (user == null)
                return NotFound();

            return View(user);
        }

        [AllowAnonymous]
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

    #region Request Models

    public class UserRoleUpdateRequest
    {
        public int UserId { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    public class UserActivationRequest
    {
        public int UserId { get; set; }
        public bool IsActive { get; set; }
    }

    #endregion
}
