using Dashboard.Web.Models;
using Dashboard.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Dashboard.Web.Controllers
{
    [Authorize]
    public class BannerController : Controller
    {
        private readonly IApiService<BannerViewModel> _bannerService;
        private readonly ILogger<BannerController> _logger;

        public BannerController(IApiService<BannerViewModel> bannerService, ILogger<BannerController> logger)
        {
            _bannerService = bannerService ?? throw new ArgumentNullException(nameof(bannerService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var banners = await _bannerService.GetAllAsync();
                return View(banners?.Data ?? new List<BannerViewModel>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading banners");
                TempData["Error"] = "Banner'lar yüklenirken hata oluştu";
                return View(new List<BannerViewModel>());
            }
        }

        public IActionResult Create()
        {
            return View(new BannerViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BannerViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var result = await _bannerService.CreateAsync(model);
                if (result.Success)
                {
                    TempData["Success"] = "Banner başarıyla oluşturuldu";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", result.Message ?? "Banner oluşturulamadı");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating banner");
                ModelState.AddModelError("", "Hata: " + ex.Message);
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var result = await _bannerService.GetByIdAsync(id);
                if (result == null || result.Data == null)
                {
                    TempData["Error"] = "Banner bulunamadı";
                    return RedirectToAction(nameof(Index));
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading banner {Id}", id);
                TempData["Error"] = "Banner yüklenirken hata oluştu";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BannerViewModel model)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var result = await _bannerService.UpdateAsync(id, model);
                if (result.Success)
                {
                    TempData["Success"] = "Banner başarıyla güncellendi";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", result.Message ?? "Banner güncellenemedi");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating banner {Id}", id);
                ModelState.AddModelError("", "Hata: " + ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _bannerService.DeleteAsync(id);
                if (result.Success)
                {
                    TempData["Success"] = "Banner başarıyla silindi";
                }
                else
                {
                    TempData["Error"] = result.Message ?? "Banner silinemedi";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting banner {Id}", id);
                TempData["Error"] = "Hata: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            try
            {
                var result = await _bannerService.GetByIdAsync(id);
                if (result != null && result.Data != null)
                {
                    result.Data.IsActive = !result.Data.IsActive;
                    await _bannerService.UpdateAsync(id, result.Data);
                    return Json(new { success = true, isActive = result.Data.IsActive });
                }
                return Json(new { success = false, message = "Banner bulunamadı" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling banner {Id}", id);
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
