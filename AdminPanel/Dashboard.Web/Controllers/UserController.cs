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

        public UserController(UserApiService userService)
        {
            _userService = userService;
        }

        // Listeleme
        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllAsync();
            return View(users);
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

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UserDto user)
        {
            if (!ModelState.IsValid)
            {
                var roles = await _userService.GetRolesAsync();
                ViewBag.AllRoles = roles ?? new List<string>();
                return View(user);
            }

            var success = await _userService.UpdateAsync(user.Id, user);

            if (success)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Kullanıcı güncellenirken hata oluştu.");
            var roleList = await _userService.GetRolesAsync();
            ViewBag.AllRoles = roleList ?? new List<string>();
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
