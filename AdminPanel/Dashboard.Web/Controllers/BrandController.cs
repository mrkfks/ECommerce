using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dashboard.Web.Services;

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
        if (!ModelState.IsValid)
        {
            await LoadCompaniesAsync();
            return View(model);
        }

        try
        {
            var brand = new BrandViewModel
            {
                Name = model.Name,
                Description = model.Description,
                LogoUrl = model.LogoUrl,
                IsActive = model.IsActive,
                CompanyId = model.CompanyId
            };

            var success = await _brandService.CreateAsync(brand);
            if (success)
            {
                TempData["Success"] = "Marka başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Marka oluşturulurken hata oluştu.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Marka oluşturulurken hata");
            TempData["Error"] = ex.Message;
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
            LogoUrl = brand.LogoUrl,
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
            var brand = new BrandViewModel
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description,
                LogoUrl = model.LogoUrl,
                IsActive = model.IsActive
            };

            var success = await _brandService.UpdateAsync(id, brand);
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
        var companies = await _companyService.GetAllAsync();
        ViewBag.Companies = companies?.Select(c => new CompanyViewModel 
        { 
            Id = c.Id, 
            Name = c.Name, 
            LogoUrl = c.LogoUrl,
            IsActive = c.IsActive 
        }).ToList() ?? new List<CompanyViewModel>();
    }
}
