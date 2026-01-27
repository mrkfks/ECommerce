using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dashboard.Web.Services;

namespace Dashboard.Web.Controllers;

/// <summary>
/// Global Attribute (Site Ayarları) yönetimi controller'ı
/// </summary>
[Authorize(Policy = "SuperAdminOnly")]
public class SettingsController : Controller
{
    private readonly GlobalAttributeApiService _attributeService;
    private readonly CompanyApiService _companyService;
    private readonly ILogger<SettingsController> _logger;

    public SettingsController(
        GlobalAttributeApiService attributeService,
        CompanyApiService companyService,
        ILogger<SettingsController> logger)
    {
        _attributeService = attributeService;
        _companyService = companyService;
        _logger = logger;
    }

    /// <summary>
    /// Ayarlar listesi
    /// </summary>
    public async Task<IActionResult> Index(string? group = null)
    {
        try
        {
            var attributes = await _attributeService.GetAllAsync();
            
            if (!string.IsNullOrEmpty(group) && attributes != null)
            {
                attributes = attributes.Where(a => a.Group == group).ToList();
            }

            // Grupları ViewBag'e ekle
            if (attributes != null)
            {
                ViewBag.Groups = attributes
                    .Where(a => !string.IsNullOrEmpty(a.Group))
                    .Select(a => a.Group)
                    .Distinct()
                    .OrderBy(g => g)
                    .ToList();
            }
            
            ViewBag.SelectedGroup = group;
            
            return View(attributes ?? new List<GlobalAttributeViewModel>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ayarlar yüklenirken hata oluştu");
            TempData["Error"] = "Ayarlar yüklenirken hata oluştu.";
            return View(new List<GlobalAttributeViewModel>());
        }
    }

    /// <summary>
    /// Yeni ayar oluşturma formu
    /// </summary>
    public async Task<IActionResult> Create()
    {
        await LoadCompaniesAsync();
        return View(new GlobalAttributeCreateViewModel());
    }

    /// <summary>
    /// Yeni ayar oluşturma
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(GlobalAttributeCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadCompaniesAsync();
            return View(model);
        }

        try
        {
            var attribute = new GlobalAttributeViewModel
            {
                Key = model.Key,
                Value = model.Value,
                Description = model.Description,
                Group = model.Group,
                CompanyId = model.CompanyId
            };

            var success = await _attributeService.CreateAsync(attribute);
            if (success)
            {
                TempData["Success"] = "Ayar başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index), new { group = model.Group });
            }

            TempData["Error"] = "Ayar oluşturulurken hata oluştu.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ayar oluşturulurken hata");
            TempData["Error"] = ex.Message;
        }

        await LoadCompaniesAsync();
        return View(model);
    }

    /// <summary>
    /// Ayar düzenleme formu
    /// </summary>
    public async Task<IActionResult> Edit(int id)
    {
        var attribute = await _attributeService.GetByIdAsync(id);
        if (attribute == null)
        {
            TempData["Error"] = "Ayar bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        await LoadCompaniesAsync();
        
        var model = new GlobalAttributeUpdateViewModel
        {
            Id = attribute.Id,
            Key = attribute.Key,
            Value = attribute.Value,
            Description = attribute.Description,
            Group = attribute.Group
        };
        
        return View(model);
    }

    /// <summary>
    /// Ayar güncelleme
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, GlobalAttributeUpdateViewModel model)
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
            var attribute = new GlobalAttributeViewModel
            {
                Id = model.Id,
                Key = model.Key,
                Value = model.Value,
                Description = model.Description,
                Group = model.Group
            };

            var success = await _attributeService.UpdateAsync(id, attribute);
            if (success)
            {
                TempData["Success"] = "Ayar başarıyla güncellendi.";
                return RedirectToAction(nameof(Index), new { group = model.Group });
            }

            TempData["Error"] = "Ayar güncellenirken hata oluştu.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ayar güncellenirken hata");
            TempData["Error"] = ex.Message;
        }

        await LoadCompaniesAsync();
        return View(model);
    }

    /// <summary>
    /// Ayar silme
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, string? group = null)
    {
        try
        {
            var success = await _attributeService.DeleteAsync(id);
            if (success)
            {
                TempData["Success"] = "Ayar başarıyla silindi.";
            }
            else
            {
                TempData["Error"] = "Ayar silinirken hata oluştu.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ayar silinirken hata");
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index), new { group });
    }

    /// <summary>
    /// Hızlı ayar güncelleme (AJAX)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> QuickUpdate([FromBody] QuickUpdateModel model)
    {
        try
        {
            var attribute = await _attributeService.GetByIdAsync(model.Id);
            if (attribute == null)
            {
                return Json(new { success = false, message = "Ayar bulunamadı." });
            }

            attribute.Value = model.Value;
            var success = await _attributeService.UpdateAsync(model.Id, attribute);
            
            return Json(new { success, message = success ? "Güncellendi" : "Hata oluştu" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Hızlı güncelleme hatası");
            return Json(new { success = false, message = ex.Message });
        }
    }

    private async Task LoadCompaniesAsync()
    {
        var response = await _companyService.GetAllAsync();
        ViewBag.Companies = response?.Data?.Select(c => new CompanyViewModel 
        { 
            Id = c.Id, 
            Name = c.Name, 
            LogoUrl = c.LogoUrl,
            IsActive = c.IsActive 
        }).ToList() ?? new List<CompanyViewModel>();
    }

    public class QuickUpdateModel
    {
        public int Id { get; set; }
        public string Value { get; set; } = string.Empty;
    }
}
