using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dashboard.Web.Services;
using Dashboard.Web.Models;
using System.Text.Json;
using ModelDto = Dashboard.Web.Models.ModelDto;
using GlobalAttributeDto = Dashboard.Web.Models.GlobalAttributeDto;
using ModelCreateDto = Dashboard.Web.Models.ModelCreateDto;
using GlobalAttributeCreateDto = Dashboard.Web.Models.GlobalAttributeCreateDto;

namespace Dashboard.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class CategoryController : Controller
    {
        private readonly IApiService<CategoryViewModel> _categoryService;
        private readonly IApiService<BrandDto> _brandService;
        private readonly IApiService<ModelDto> _modelService;
        private readonly IApiService<GlobalAttributeDto> _globalAttributeService;

        public CategoryController(
            IApiService<CategoryViewModel> categoryService,
            IApiService<BrandDto> brandService,
            IApiService<ModelDto> modelService,
            IApiService<GlobalAttributeDto> globalAttributeService)
        {
            _categoryService = categoryService;
            _brandService = brandService;
            _modelService = modelService;
            _globalAttributeService = globalAttributeService;
        }

        // Listeleme
        public async Task<IActionResult> Index()
        {
            var response = await _categoryService.GetAllAsync();
            if (response == null || response.Data == null)
                return View(new List<CategoryViewModel>());
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
            var categoriesResponse = await _categoryService.GetAllAsync();
            var categories = categoriesResponse?.Data ?? new List<CategoryViewModel>();
            var viewModel = new CategoryViewModel
            {
                AvailableParentCategories = categories.Select(x => new CategoryDto {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    ImageUrl = x.ImageUrl,
                    IsActive = x.IsActive,
                    ProductCount = x.ProductCount,
                    ParentCategoryId = x.ParentCategoryId,
                    DisplayOrder = x.DisplayOrder,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                }).ToList()
            };
            return View(viewModel);
        }

        // Ana kategori ekleme - POST (Basit)
        [HttpPost]
            public async Task<IActionResult> CreateCategory(CategoryDto categoryDto, IFormFile? ImageFile)
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
                    
                        categoryDto.ImageUrl = $"/uploads/categories/{fileName}";
                    }
                
                categoryDto.ParentCategoryId = null; // Ana kategori
                var success = await _categoryService.CreateAsync(categoryDto);
                
                if (success)
                {
                        TempData["Success"] = "Ana kategori başarıyla eklendi.";
                }
                else
                {
                        TempData["Error"] = "Kategori eklenirken hata oluştu.";
                }
            }
            catch (Exception ex)
            {
                    TempData["Error"] = $"Hata: {ex.Message}";
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
                    
                        categoryDto.ImageUrl = $"/uploads/categories/{fileName}";
                    }
                
                if (!categoryDto.ParentCategoryId.HasValue)
                {
                        TempData["Error"] = "Üst kategori seçilmelidir.";
                    return RedirectToAction(nameof(Index));
                }

                var success = await _categoryService.CreateAsync(categoryDto);
                
                if (success)
                {
                        TempData["Success"] = "Alt kategori başarıyla eklendi.";
                }
                else
                {
                        TempData["Error"] = "Alt kategori eklenirken hata oluştu.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Hata: {ex.Message}";
            }
            
                return RedirectToAction(nameof(Index));
            }
        // Recursive kategori oluşturma methodları kaldırıldı (Eski kodlar temizlendi)

        /// Düzen Leme - GET
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var categoryResponse = await _categoryService.GetByIdAsync(id);
            var category = categoryResponse?.Data;
            if (category == null)
                return NotFound();

            // Alt kategorileri yükle
            var categoriesResponse = await _categoryService.GetAllAsync();
            var categories = categoriesResponse?.Data ?? new List<CategoryViewModel>();
            ViewBag.SubCategories = categories.Where(c => c.ParentCategoryId == id).ToList();

            return View(category);
        }

        // Düzenleme - POST
        [HttpPost]
        public async Task<IActionResult> Edit(CategoryDto category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }

            var success = await _categoryService.UpdateAsync(category.Id, category);

            if (success)
            {
                TempData["Success"] = "Kategori başarıyla güncellendi";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Kategori güncellenirken hata oluştu.");
            return View(category);
        }

        // Silme - GET
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null)
                return NotFound();

            return View(category);
        }

        // Silme - POST
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deleteResponse = await _categoryService.DeleteAsync(id);
            var success = deleteResponse != null && deleteResponse.Success;

            if (success)
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
        public async Task<IActionResult> CreateBrand(BrandDto brandDto, IFormFile? ImageFile)
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

                    brandDto.ImageUrl = $"/uploads/brands/{fileName}";
                }

                // Zorunlu alan koruması
                brandDto.Description ??= string.Empty;

                // Kategori bağımsız marka için CategoryId boş bırakılır
                brandDto.CategoryId = null;

                var response = await _brandService.CreateAsync(brandDto);
                var success = response != null && response.Success;

                TempData[success ? "Success" : "Error"] = success
                    ? "Marka başarıyla eklendi."
                    : "Marka eklenirken hata oluştu.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Hata: {ex.Message}";
            }

            return RedirectToAction(nameof(Brands));
        }

        // Marka Yönetimi
        public async Task<IActionResult> Brands()
        {
            var brands = await _brandService.GetAllAsync();
            return View(brands);
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
        public async Task<IActionResult> EditBrand(int id, BrandDto brandDto, IFormFile? ImageFile)
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

                    brandDto.ImageUrl = $"/uploads/brands/{fileName}";
                }

                brandDto.Description ??= string.Empty;
                brandDto.CategoryId = null;

                var response = await _brandService.UpdateAsync(id, brandDto);
                var success = response != null && response.Success;

                TempData[success ? "Success" : "Error"] = success
                    ? "Marka başarıyla güncellendi."
                    : "Marka güncellenirken hata oluştu.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Hata: {ex.Message}";
            }

            return RedirectToAction(nameof(Brands));
        }

        // Marka silme - POST
        [HttpPost]
        public async Task<IActionResult> DeleteBrand(int id)
        {
            try
            {
                var response = await _brandService.DeleteAsync(id);
                var success = response != null && response.Success;
                TempData[success ? "Success" : "Error"] = success
                    ? "Marka başarıyla silindi."
                    : "Marka silinirken hata oluştu.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Hata: {ex.Message}";
            }

            return RedirectToAction(nameof(Brands));
        }

        // Model ekleme - POST
        [HttpPost]
        public async Task<IActionResult> CreateModel(int brandId, [FromBody] ModelCreateDto modelDto)
        {
            try
            {
                if (modelDto == null)
                {
                    return Json(new { success = false, message = "Model bilgileri boş" });
                }
                
                // ModelCreateDto'dan ModelDto'ya dönüştür (BrandId ile birlikte yeni model oluştur)
                var model = new ModelDto
                {
                    BrandId = brandId,
                    Name = modelDto.Name,
                    Description = modelDto.Description ?? string.Empty,
                    ImageUrl = modelDto.ImageUrl,
                    IsActive = modelDto.IsActive,
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
                
                TempData[success ? "Success" : "Error"] = success
                    ? "Model başarıyla eklendi."
                    : "Model eklenirken hata oluştu.";
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
                
                TempData["Error"] = $"Hata: {ex.Message}";
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
                
                TempData["Success"] = "Model başarıyla silindi.";
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
                
                TempData["Error"] = $"Hata: {ex.Message}";
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
        public async Task<IActionResult> CreateAttribute(GlobalAttributeCreateDto dto)
        {
            // Values null ise boş liste kullan, değilse filtrele
            dto.Values = dto.Values?.Where(v => !string.IsNullOrWhiteSpace(v.Value)).ToList() ?? new List<GlobalAttributeValueFormDto>();
            
            var success = await _globalAttributeService.CreateAsync<GlobalAttributeCreateDto>(dto);
            TempData[success ? "Success" : "Error"] = success ? "Özellik eklendi." : "Özellik eklenemedi.";
            return RedirectToAction(nameof(Attributes));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAttribute(int id)
        {
            var result = await _globalAttributeService.DeleteAsync(id);
            TempData[result.Success ? "Success" : "Error"] = result.Success ? "Özellik silindi." : "Özellik silinemedi.";
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
