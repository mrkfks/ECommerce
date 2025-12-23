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

        public UserController(UserApiService userService, CompanyApiService companyService)
        {
            _userService = userService;
            _companyService = companyService;
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
    }
}
