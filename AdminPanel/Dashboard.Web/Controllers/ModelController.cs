using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dashboard.Web.Services;

namespace Dashboard.Web.Controllers;

/// <summary>
/// Model yönetimi controller'ı (Ürün modelleri - marka altında)
/// </summary>
[Authorize(Policy = "AdminOnly")]
public class ModelController : Controller
{
    private readonly ModelApiService _modelService;
    private readonly BrandApiService _brandService;
    private readonly ILogger<ModelController> _logger;

    public ModelController(
        ModelApiService modelService,
        BrandApiService brandService,
        ILogger<ModelController> logger)
    {
        _modelService = modelService;
        _brandService = brandService;
        _logger = logger;
    }

    /// <summary>
    /// Model listesi
    /// </summary>
    public async Task<IActionResult> Index(int? brandId = null)
    {
        try
        {
            List<ModelViewModel>? models;
            
            if (brandId.HasValue)
            {
                models = await _modelService.GetByBrandIdAsync(brandId.Value);
                var brand = await _brandService.GetByIdAsync(brandId.Value);
                ViewBag.SelectedBrand = brand;
            }
            else
            {
                models = await _modelService.GetAllAsync();
            }

            await LoadBrandsAsync();
            return View(models ?? new List<ModelViewModel>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Modeller yüklenirken hata oluştu");
            TempData["Error"] = "Modeller yüklenirken hata oluştu.";
            return View(new List<ModelViewModel>());
        }
    }

    /// <summary>
    /// Model detayı
    /// </summary>
    public async Task<IActionResult> Details(int id)
    {
        var model = await _modelService.GetByIdAsync(id);
        if (model == null)
        {
            TempData["Error"] = "Model bulunamadı.";
            return RedirectToAction(nameof(Index));
        }
        return View(model);
    }

    /// <summary>
    /// Yeni model oluşturma formu
    /// </summary>
    public async Task<IActionResult> Create(int? brandId = null)
    {
        await LoadBrandsAsync();
        
        return View(new ModelCreateViewModel 
        { 
            BrandId = brandId ?? 0 
        });
    }

    /// <summary>
    /// Yeni model oluşturma
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ModelCreateViewModel createModel)
    {
        if (!ModelState.IsValid)
        {
            await LoadBrandsAsync();
            return View(createModel);
        }

        try
        {
            var model = new ModelViewModel
            {
                Name = createModel.Name,
                Description = createModel.Description,
                BrandId = createModel.BrandId,
                IsActive = createModel.IsActive
            };

            var success = await _modelService.CreateAsync(model);
            if (success)
            {
                TempData["Success"] = "Model başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index), new { brandId = createModel.BrandId });
            }

            TempData["Error"] = "Model oluşturulurken hata oluştu.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Model oluşturulurken hata");
            TempData["Error"] = ex.Message;
        }

        await LoadBrandsAsync();
        return View(createModel);
    }

    /// <summary>
    /// Model düzenleme formu
    /// </summary>
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _modelService.GetByIdAsync(id);
        if (model == null)
        {
            TempData["Error"] = "Model bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        await LoadBrandsAsync();
        
        var updateModel = new ModelUpdateViewModel
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            BrandId = model.BrandId,
            IsActive = model.IsActive
        };
        
        return View(updateModel);
    }

    /// <summary>
    /// Model güncelleme
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ModelUpdateViewModel updateModel)
    {
        if (id != updateModel.Id)
        {
            TempData["Error"] = "Geçersiz istek.";
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            await LoadBrandsAsync();
            return View(updateModel);
        }

        try
        {
            var model = new ModelViewModel
            {
                Id = updateModel.Id,
                Name = updateModel.Name,
                Description = updateModel.Description,
                BrandId = updateModel.BrandId,
                IsActive = updateModel.IsActive
            };

            var success = await _modelService.UpdateAsync(id, model);
            if (success)
            {
                TempData["Success"] = "Model başarıyla güncellendi.";
                return RedirectToAction(nameof(Index), new { brandId = updateModel.BrandId });
            }

            TempData["Error"] = "Model güncellenirken hata oluştu.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Model güncellenirken hata");
            TempData["Error"] = ex.Message;
        }

        await LoadBrandsAsync();
        return View(updateModel);
    }

    /// <summary>
    /// Model silme
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int? brandId = null)
    {
        try
        {
            var success = await _modelService.DeleteAsync(id);
            if (success)
            {
                TempData["Success"] = "Model başarıyla silindi.";
            }
            else
            {
                TempData["Error"] = "Model silinirken hata oluştu.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Model silinirken hata");
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index), new { brandId });
    }

    /// <summary>
    /// AJAX: Markaya göre model listesi
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetByBrand(int brandId)
    {
        var models = await _modelService.GetByBrandIdAsync(brandId);
        return Json(models ?? new List<ModelViewModel>());
    }

    private async Task LoadBrandsAsync()
    {
        var brands = await _brandService.GetAllAsync();
        ViewBag.Brands = brands ?? new List<BrandViewModel>();
    }
}
