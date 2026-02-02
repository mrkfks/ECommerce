
using Dashboard.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dashboard.Web.Services;
using AppBrandDto = ECommerce.Application.DTOs.BrandDto;
using ECommerce.Application.DTOs;
using System.Text.Json;

namespace Dashboard.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class CategoryController : Controller
    {
        private readonly IApiService<CategoryDto> _categoryService;
        private readonly IApiService<CategoryFormDto> _categoryFormService;
        private readonly IApiService<AppBrandDto> _brandService;
        private readonly IApiService<ModelDto> _modelService;
        private readonly IApiService<GlobalAttributeDto> _globalAttributeService;
        private readonly IApiService<GlobalAttributeFormDto> _globalAttributeFormService;

        public CategoryController(
            IApiService<CategoryDto> categoryService,
            IApiService<CategoryFormDto> categoryFormService,
            IApiService<AppBrandDto> brandService,
            IApiService<ModelDto> modelService,
            IApiService<GlobalAttributeDto> globalAttributeService,
            IApiService<GlobalAttributeFormDto> globalAttributeFormService)
        {
            _categoryService = categoryService;
            _categoryFormService = categoryFormService;
            _brandService = brandService;
            _modelService = modelService;
            _globalAttributeService = globalAttributeService;
            _globalAttributeFormService = globalAttributeFormService;
        }

        // Listeleme
        public async Task<IActionResult> Index()
        {
            var response = await _categoryService.GetAllAsync();
            if (response == null || response.Data == null)
                return View(new List<CategoryDto>());
            return View(response.Data);
        }

        // Detay
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var response = await _categoryService.GetByIdAsync(id);
            if (response == null || response.Data == null)
                return NotFound();
            return View(response.Data);
        }

        // Yeni kategori ekleme - GET
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var allCategoriesResponse = await _categoryService.GetAllAsync();
            return View(allCategoriesResponse.Data ?? new List<CategoryDto>());
        }

        // Ana kategori ekleme - POST (Basit)
        [HttpPost]
        public async Task<IActionResult> CreateCategory(CategoryFormDto categoryFormDto, IFormFile? ImageFile)
        {
            try
            {
                // Dosya yükleme işlemi
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var fileName = $"{Guid.NewGuid()}_{ImageFile.FileName}";
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "categories");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);
                    var filePath = Path.Combine(uploadsFolder, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }
                    categoryFormDto = categoryFormDto with { ImageUrl = $"/uploads/categories/{fileName}" };
                }

                categoryFormDto = categoryFormDto with { ParentCategoryId = null }; // Ana kategori
                var response = await _categoryFormService.CreateAsync(categoryFormDto);
                TempData[response.Success ? "Success" : "Error"] = response.Success
                    ? "Ana kategori başarıyla eklendi."
                    : $"Kategori eklenirken hata oluştu: {response.Message}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Hata: {ex.Message}";
                System.IO.File.AppendAllText(
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "logs", "category-error-log.txt"),
                    $"[{DateTime.Now}] CreateCategory Exception: {ex.Message}\n{ex.StackTrace}\n");
            }
            return RedirectToAction(nameof(Index));
        }

        // Alt kategori ekleme - POST
        [HttpPost]
            public async Task<IActionResult> CreateSubCategory(CategoryDto categoryDto, IFormFile? ImageFile)
        {
            try
            {
                    // Dosya yükleme işlemi
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        var fileName = $"{Guid.NewGuid()}_{ImageFile.FileName}";
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "categories");
                    
                        // Klasör yoksa oluştur
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);
                    
                        var filePath = Path.Combine(uploadsFolder, fileName);
                    
                        using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                            await ImageFile.CopyToAsync(stream);
                    }
                    
                        categoryDto = categoryDto with { ImageUrl = $"/uploads/categories/{fileName}" };
                    }
                
                if (!categoryDto.ParentCategoryId.HasValue)
                {
                        TempData["Error"] = "Üst kategori seçilmelidir.";
                    return RedirectToAction(nameof(Index));
                }

                var response = await _categoryService.CreateAsync(categoryDto);
                TempData[response.Success ? "Success" : "Error"] = response.Success ? "Alt kategori başarıyla eklendi." : $"Alt kategori eklenirken hata oluştu: {response.Message}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Hata: {ex.Message}";
            }
            
                return RedirectToAction(nameof(Index));
            }
        // Recursive kategori oluşturma methodları kaldırıldı (Eski kodlar temizlendi)

        // Düzenleme - GET
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var categoryResponse = await _categoryService.GetByIdAsync(id);
            if (categoryResponse == null || categoryResponse.Data == null)
                return NotFound();

            // Alt kategorileri yükle
            var categories = await _categoryService.GetAllAsync();
            ViewBag.SubCategories = categories.Data?.Where(c => c.ParentCategoryId == id).ToList() ?? new List<CategoryDto>();

            return View(categoryResponse.Data);
        }

        // Düzenleme - POST
        [HttpPost]
        public async Task<IActionResult> Edit(CategoryDto category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }

            var response = await _categoryService.UpdateAsync(category.Id, category);
            if (response.Success)
            {
                TempData["Success"] = "Kategori başarıyla güncellendi";
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("", $"Kategori güncellenirken hata oluştu: {response.Message}");
            return View(category);
        }

        // Silme - GET
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var categoryResponse = await _categoryService.GetByIdAsync(id);
            if (categoryResponse == null || categoryResponse.Data == null)
                return NotFound();

            return View(categoryResponse.Data);
        }

        // Silme - POST
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _categoryService.DeleteAsync(id);
            if (result.Success)
            {
                TempData["Success"] = "Kategori başarıyla silindi";
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("", "Kategori silinirken hata oluştu.");
            var categoryResponse = await _categoryService.GetByIdAsync(id);
            return View(categoryResponse?.Data);
        }

        // Marka ekleme - POST
        [HttpPost]
        public async Task<IActionResult> CreateBrand(AppBrandDto brandDto, IFormFile? ImageFile)
        {
            try
            {
                // Dosya yükleme işlemi
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var fileName = $"{Guid.NewGuid()}_{ImageFile.FileName}";
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "brands");

                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var filePath = Path.Combine(uploadsFolder, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    brandDto = brandDto with { ImageUrl = $"/uploads/brands/{fileName}" };
                }

                // Zorunlu alan koruması ve init-only property için yeni nesne oluştur
                var companyId = brandDto.CompanyId != 0 ? brandDto.CompanyId : 1; // Gerekirse oturumdan veya default
                var categoryId = brandDto.CategoryId ?? 1; // Gerekirse seçili kategori veya default
                var newBrandDto = brandDto with {
                    Description = brandDto.Description ?? string.Empty,
                    CompanyId = companyId,
                    CategoryId = categoryId
                };
                var result = await _brandService.CreateAsync(newBrandDto);
                if (result.Success)
                {
                    TempData["InventoryAlert"] = "Marka başarıyla eklendi.";
                    TempData["InventoryAlertType"] = "success";
                }
                else
                {
                    TempData["InventoryAlert"] = $"Marka eklenirken hata oluştu: {result.Message}";
                    TempData["InventoryAlertType"] = "error";
                }
            }
            catch (Exception ex)
            {
                TempData["InventoryAlert"] = $"Hata: {ex.Message}";
                TempData["InventoryAlertType"] = "error";
                // Hata ve gönderilen veriyi logla
                var logPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "logs", "brand-error-log.txt");
                var logText = $"[{DateTime.Now}] CreateBrand Exception: {ex.Message}\n{ex.StackTrace}\nBrandDto: {System.Text.Json.JsonSerializer.Serialize(brandDto)}\n";
                System.IO.File.AppendAllText(logPath, logText);
            }

            return RedirectToAction(nameof(Brands));
        }

        // Marka Yönetimi
        public async Task<IActionResult> Brands()
        {
            var brands = await _brandService.GetAllAsync();
            return View(brands.Data ?? new List<ECommerce.Application.DTOs.BrandDto>());
        }

        // Marka düzenleme - GET (JSON döndürür)
        [HttpGet]
        public async Task<IActionResult> GetBrand(int id)
        {
            var brand = await _brandService.GetByIdAsync(id);
            if (brand == null)
                return NotFound();
            
            var models = await _modelService.GetListAsync($"brand/{id}");
            return Json(new { brand, models });
        }

        // Marka düzenleme - POST
        [HttpPost]
        public async Task<IActionResult> EditBrand(int id, AppBrandDto brandDto, IFormFile? ImageFile)
        {
            try
            {
                string? newImageUrl = null;
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var fileName = $"{Guid.NewGuid()}_{ImageFile.FileName}";
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "brands");

                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var filePath = Path.Combine(uploadsFolder, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }
                    newImageUrl = $"/uploads/brands/{fileName}";
                }

                var updatedBrandDto = brandDto with {
                    Description = brandDto.Description ?? string.Empty,
                    CategoryId = null,
                    ImageUrl = newImageUrl ?? brandDto.ImageUrl
                };
                var result = await _brandService.UpdateAsync(id, updatedBrandDto);
                TempData["InventoryAlert"] = result.Success
                    ? "Marka başarıyla güncellendi."
                    : "Marka güncellenirken hata oluştu.";
                TempData["InventoryAlertType"] = result.Success ? "success" : "error";
            }
            catch (Exception ex)
            {
                TempData["InventoryAlert"] = $"Hata: {ex.Message}";
                TempData["InventoryAlertType"] = "error";
            }

            return RedirectToAction(nameof(Brands));
        }

        // Marka silme - POST
        [HttpPost]
        public async Task<IActionResult> DeleteBrand(int id)
        {
            try
            {
                var result = await _brandService.DeleteAsync(id);
                TempData["InventoryAlert"] = result.Success
                    ? "Marka başarıyla silindi."
                    : "Marka silinirken hata oluştu.";
                TempData["InventoryAlertType"] = result.Success ? "success" : "error";
            }
            catch (Exception ex)
            {
                TempData["InventoryAlert"] = $"Hata: {ex.Message}";
                TempData["InventoryAlertType"] = "error";
            }

            return RedirectToAction(nameof(Brands));
        }

        // Model ekleme - POST
        [HttpPost]
        public async Task<IActionResult> CreateModel(int brandId, [FromBody] ModelFormDto modelDto)
        {
            try
            {
                if (modelDto == null)
                {
                    return Json(new { success = false, message = "Model bilgileri boş" });
                }
                
                var modelForm = new ModelFormDto
                {
                    Id = modelDto.Id,
                    BrandId = brandId,
                    Name = modelDto.Name,
                    Description = modelDto.Description,
                    ImageUrl = modelDto.ImageUrl,
                    IsActive = modelDto.IsActive
                };
                
                // ModelCreateDto'dan ModelDto'ya dönüştür
                var model = new ModelDto
                {
                    BrandId = modelForm.BrandId,
                    Name = modelForm.Name,
                    Description = modelForm.Description,
                    ImageUrl = modelForm.ImageUrl,
                    IsActive = modelForm.IsActive,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                var response = await _modelService.CreateAsync(model);
                var success = response != null && response.Success;
                
                // AJAX isteği ise sadece JSON döndür
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || 
                    Request.Headers["Accept"].ToString().Contains("application/json"))
                {
                    return Json(new { success, message = success ? "Model başarıyla eklendi." : "Model eklenirken hata oluştu." });
                }
                
                TempData["InventoryAlert"] = success
                    ? "Model başarıyla eklendi."
                    : "Model eklenirken hata oluştu.";
                TempData["InventoryAlertType"] = success ? "success" : "error";
                return RedirectToAction(nameof(Brands));
            }
            catch (Exception ex)
            {
                // AJAX isteği ise sadece JSON döndür
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || 
                    Request.Headers["Accept"].ToString().Contains("application/json"))
                {
                    return Json(new { success = false, message = $"Hata: {ex.Message}" });
                }
                
                TempData["InventoryAlert"] = $"Hata: {ex.Message}";
                TempData["InventoryAlertType"] = "error";
                return RedirectToAction(nameof(Brands));
            }
        }

        // Model silme - POST
        [HttpPost]
        public async Task<IActionResult> DeleteModel(int id)
        {
            try
            {
                await _modelService.DeleteAsync(id);
                
                // AJAX isteği ise sadece JSON döndür
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || 
                    Request.Headers["Accept"].ToString().Contains("application/json"))
                {
                    return Json(new { success = true, message = "Model başarıyla silindi." });
                }
                
                TempData["InventoryAlert"] = "Model başarıyla silindi.";
                TempData["InventoryAlertType"] = "success";
                return RedirectToAction(nameof(Brands));
            }
            catch (Exception ex)
            {
                // AJAX isteği ise sadece JSON döndür
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || 
                    Request.Headers["Accept"].ToString().Contains("application/json"))
                {
                    return Json(new { success = false, message = $"Hata: {ex.Message}" });
                }
                
                TempData["InventoryAlert"] = $"Hata: {ex.Message}";
                TempData["InventoryAlertType"] = "error";
                return RedirectToAction(nameof(Brands));
            }
        }

        // Özellik Yönetimi
        public async Task<IActionResult> Attributes()
        {
            var response = await _globalAttributeService.GetAllAsync();
            var attributes = response?.Data ?? new List<GlobalAttributeDto>();
            return View(attributes);
        }

        [HttpPost]

        [HttpPost]
        public async Task<IActionResult> CreateAttribute(ECommerce.Application.DTOs.GlobalAttributeFormDto dto)
        {
            var response = await _globalAttributeFormService.CreateAsync(dto);
            TempData["InventoryAlert"] = response != null && response.Success ? "Özellik eklendi." : $"Özellik eklenemedi: {response?.Message}";
            TempData["InventoryAlertType"] = response != null && response.Success ? "success" : "error";
            return RedirectToAction(nameof(Attributes));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAttribute(int id)
        {
            var result = await _globalAttributeService.DeleteAsync(id);
            TempData["InventoryAlert"] = result.Success ? "Özellik silindi." : "Özellik silinemedi.";
            TempData["InventoryAlertType"] = result.Success ? "success" : "error";
            return RedirectToAction(nameof(Attributes));
        }
    }

    // Form verisi için model sınıfları
    public class CategoryTreeFormData
    {
        public List<CategoryTreeNode> Categories { get; set; } = new();
        public List<BrandItemVm> Brands { get; set; } = new();
        public List<AttributeItemVm> Attributes { get; set; } = new();
    }

    public class CategoryTreeNode
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public List<CategoryTreeNode> SubCategories { get; set; } = new();
    }
}
