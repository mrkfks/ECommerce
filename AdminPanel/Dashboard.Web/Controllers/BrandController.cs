using Dashboard.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dashboard.Web.Controllers;

/// <summary>
/// Marka yönetimi controller'ı
/// </summary>
[Authorize(Policy = "AdminOnly")]
public class BrandController : Controller
{
    private readonly BrandApiService _brandService;
    private readonly CompanyApiService _companyService;
    private readonly ILogger<BrandController> _logger;

    public BrandController(
        BrandApiService brandService,
        CompanyApiService companyService,
        ILogger<BrandController> logger)
    {
        _brandService = brandService;
        _companyService = companyService;
        _logger = logger;
    }

    /// <summary>
    /// Marka listesi
    /// </summary>
    public async Task<IActionResult> Index()
    {
        try
        {
            var brands = await _brandService.GetAllAsync();
            return View(brands ?? new List<BrandViewModel>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Markalar yüklenirken hata oluştu");
            TempData["Error"] = "Markalar yüklenirken hata oluştu.";
            return View(new List<BrandViewModel>());
        }
    }

    /// <summary>
    /// Marka detayı
    /// </summary>
    public async Task<IActionResult> Details(int id)
    {
        var brand = await _brandService.GetByIdAsync(id);
        if (brand == null)
        {
            TempData["Error"] = "Marka bulunamadı.";
            return RedirectToAction(nameof(Index));
        }
        return View(brand);
    }

    /// <summary>
    /// Yeni marka oluşturma formu
    /// </summary>
    public async Task<IActionResult> Create()
    {
        await LoadCompaniesAsync();
        return View(new BrandCreateViewModel());
    }

    /// <summary>
    /// Yeni marka oluşturma
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BrandCreateViewModel model)
    {
        _logger.LogInformation($"[BrandController.Create] Received: Name={model.Name}, Description={model.Description}, CompanyId={model.CompanyId}, IsActive={model.IsActive}");

        if (!ModelState.IsValid)
        {
            var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors));
            _logger.LogWarning($"[BrandController.Create] ModelState invalid: {errors}");
            TempData["CreateError"] = "Form hatası: " + errors;
            await LoadCompaniesAsync();
            return View(model);
        }

        try
        {
            _logger.LogInformation($"[BrandController.Create] Calling BrandApiService.CreateAsync");
            var success = await _brandService.CreateAsync(model);

            if (success)
            {
                _logger.LogInformation($"[BrandController.Create] Success - Marka oluşturuldu");
                TempData["Success"] = "Marka başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }

            _logger.LogError($"[BrandController.Create] API returned false - Marka oluşturulamadı");
            TempData["CreateError"] = "Marka oluşturulurken hata oluştu. Lütfen tekrar deneyiniz.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Marka oluşturulurken exception");
            TempData["CreateError"] = $"Hata: {ex.GetBaseException().Message}";
        }

        await LoadCompaniesAsync();
        return View(model);
    }

    /// <summary>
    /// Marka düzenleme formu
    /// </summary>
    public async Task<IActionResult> Edit(int id)
    {
        var brand = await _brandService.GetByIdAsync(id);
        if (brand == null)
        {
            TempData["Error"] = "Marka bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        await LoadCompaniesAsync();

        var model = new BrandUpdateViewModel
        {
            Id = brand.Id,
            Name = brand.Name,
            Description = brand.Description,
            IsActive = brand.IsActive
        };

        return View(model);
    }

    /// <summary>
    /// Marka güncelleme
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BrandUpdateViewModel model)
    {
        if (id != model.Id)
        {
            TempData["Error"] = "Geçersiz istek.";
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            await LoadCompaniesAsync();
            return View(model);
        }

        try
        {
            var success = await _brandService.UpdateAsync(model);
            if (success)
            {
                TempData["Success"] = "Marka başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Marka güncellenirken hata oluştu.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Marka güncellenirken hata");
            TempData["Error"] = ex.Message;
        }

        await LoadCompaniesAsync();
        return View(model);
    }

    /// <summary>
    /// Marka silme
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var success = await _brandService.DeleteAsync(id);
            if (success)
            {
                TempData["Success"] = "Marka başarıyla silindi.";
            }
            else
            {
                TempData["Error"] = "Marka silinirken hata oluştu.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Marka silinirken hata");
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// AJAX: Marka listesi
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var brands = await _brandService.GetAllAsync();
        return Json(brands ?? new List<BrandViewModel>());
    }

    private async Task LoadCompaniesAsync()
    {
        var companiesResponse = await _companyService.GetAllAsync();
        ViewBag.Companies = companiesResponse?.Data?.Select(c => new CompanyViewModel
        {
            Id = c.Id,
            Name = c.Name,
            LogoUrl = c.LogoUrl,
            IsActive = c.IsActive
        }).ToList() ?? new List<CompanyViewModel>();
    }
}
