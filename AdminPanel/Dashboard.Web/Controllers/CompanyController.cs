using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dashboard.Web.Services;

namespace Dashboard.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class CompanyController : Controller
    {
        private readonly CompanyApiService _companyService;

        public CompanyController(CompanyApiService companyService)
        {
            _companyService = companyService;
        }

        public async Task<IActionResult> Index()
        {
            var companies = await _companyService.GetAllAsync();
            return View(companies);
        }

        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var success = await _companyService.ApproveAsync(id);
            TempData[success ? "Success" : "Error"] = success ? "Şirket onaylandı" : "Onay işlemi başarısız";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Deactivate(int id)
        {
            var success = await _companyService.DeactivateAsync(id);
            TempData[success ? "Success" : "Error"] = success ? "Şirket pasifleştirildi" : "Pasifleştirme başarısız";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Activate(int id)
        {
            var success = await _companyService.ActivateAsync(id);
            TempData[success ? "Success" : "Error"] = success ? "Şirket aktif hale getirildi" : "Aktivasyon başarısız";
            return RedirectToAction(nameof(Index));
        }
    }
}
