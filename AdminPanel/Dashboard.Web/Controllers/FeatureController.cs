using Dashboard.Web.Models;
using Dashboard.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dashboard.Web.Controllers;

/// <summary>
/// Özellik yönetimi controller'ı
/// </summary>
[Authorize(Policy = "AdminOnly")]
public class FeatureController : Controller
{
    private readonly FeatureApiService _featureService;
    private readonly ILogger<FeatureController> _logger;

    public FeatureController(
        FeatureApiService featureService,
        ILogger<FeatureController> logger)
    {
        _featureService = featureService;
        _logger = logger;
    }

    /// <summary>
    /// Özellik listesi
    /// </summary>
    public async Task<IActionResult> Index()
    {
        try
        {
            var features = await _featureService.GetAllAsync();
            return View(features ?? new List<FeatureViewModel>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Özellikler yüklenirken hata oluştu");
            TempData["Error"] = "Özellikler yüklenirken hata oluştu.";
            return View(new List<FeatureViewModel>());
        }
    }

    /// <summary>
    /// Özellik detayı
    /// </summary>
    public async Task<IActionResult> Details(int id)
    {
        var feature = await _featureService.GetByIdAsync(id);
        if (feature == null)
        {
            TempData["Error"] = "Özellik bulunamadı.";
            return RedirectToAction(nameof(Index));
        }
        return View(feature);
    }

    /// <summary>
    /// Yeni özellik oluşturma formu
    /// </summary>
    public IActionResult Create()
    {
        return View(new FeatureCreateViewModel());
    }

    /// <summary>
    /// Yeni özellik oluşturma
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FeatureCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var feature = new FeatureViewModel
            {
                Name = model.Name,
                Description = model.Description,
                DisplayOrder = model.DisplayOrder,
                IsActive = model.IsActive,
                Values = model.Values?
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .Select((v, i) => new FeatureValueViewModel
                    {
                        Value = v.Trim(),
                        DisplayOrder = i,
                        IsActive = true
                    }).ToList() ?? new List<FeatureValueViewModel>()
            };

            var success = await _featureService.CreateAsync(feature);
            if (success)
            {
                TempData["Success"] = "Özellik başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }

            TempData["CreateError"] = "Özellik oluşturulurken hata oluştu.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Özellik oluşturulurken hata");
            TempData["CreateError"] = ex.Message;
        }

        return View(model);
    }

    /// <summary>
    /// Özellik düzenleme formu
    /// </summary>
    public async Task<IActionResult> Edit(int id)
    {
        var feature = await _featureService.GetByIdAsync(id);
        if (feature == null)
        {
            TempData["Error"] = "Özellik bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        var model = new FeatureUpdateViewModel
        {
            Id = feature.Id,
            Name = feature.Name,
            Description = feature.Description,
            DisplayOrder = feature.DisplayOrder,
            IsActive = feature.IsActive,
            Values = feature.Values?.Select(v => new FeatureValueViewModel
            {
                Id = v.Id,
                Value = v.Value,
                DisplayOrder = v.DisplayOrder,
                IsActive = v.IsActive
            }).ToList() ?? new List<FeatureValueViewModel>()
        };

        return View(model);
    }

    /// <summary>
    /// Özellik güncelleme
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, FeatureUpdateViewModel model)
    {
        if (id != model.Id)
        {
            TempData["Error"] = "Geçersiz istek.";
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            // Mevcut değerler + yeni eklenen değerleri birleştir
            var allValues = new List<FeatureValueViewModel>();

            // Mevcut değerleri aktar
            if (model.Values != null)
            {
                allValues.AddRange(model.Values.Select(v => new FeatureValueViewModel
                {
                    Id = v.Id,
                    Value = v.Value,
                    DisplayOrder = v.DisplayOrder,
                    IsActive = v.IsActive
                }));
            }

            // Yeni değerleri ekle
            if (model.NewValues != null)
            {
                var maxOrder = allValues.Any() ? allValues.Max(v => v.DisplayOrder) + 1 : 0;
                allValues.AddRange(model.NewValues
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .Select((v, i) => new FeatureValueViewModel
                    {
                        Value = v.Trim(),
                        DisplayOrder = maxOrder + i,
                        IsActive = true
                    }));
            }

            var feature = new FeatureViewModel
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description,
                DisplayOrder = model.DisplayOrder,
                IsActive = model.IsActive,
                Values = allValues
            };

            var success = await _featureService.UpdateAsync(id, feature);
            if (success)
            {
                TempData["Success"] = "Özellik başarıyla güncellenmiştir.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Özellik güncellenirken hata oluştu.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Özellik güncellenirken hata");
            TempData["Error"] = ex.Message;
        }

        return View(model);
    }

    /// <summary>
    /// Özellik silme
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var success = await _featureService.DeleteAsync(id);
            if (success)
            {
                TempData["Success"] = "Özellik başarıyla silindi.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Özellik silinirken hata oluştu.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Özellik silinirken hata");
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
