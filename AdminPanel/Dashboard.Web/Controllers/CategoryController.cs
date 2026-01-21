using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.DTOs;
using Dashboard.Web.Services;
using Dashboard.Web.Models;
using System.Text.Json;

namespace Dashboard.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class CategoryController : Controller
    {
        private readonly IApiService<CategoryDto> _categoryService;
        private readonly IApiService<BrandDto> _brandService;
        private readonly ModelApiService _modelService;
        private readonly GlobalAttributeApiService _globalAttributeService;

        public CategoryController(
            IApiService<CategoryDto> categoryService,
            IApiService<BrandDto> brandService,
            ModelApiService modelService,
            GlobalAttributeApiService globalAttributeService)
        {
            _categoryService = categoryService;
            _brandService = brandService;
            _modelService = modelService;
            _globalAttributeService = globalAttributeService;
        }

        // Listeleme
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllAsync();
            return View(categories);
        }

        // Detay
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null)
                return NotFound();

            return View(category);
        }

        // Yeni kategori ekleme - GET
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var viewModel = new CategoryViewModel
            {
                AvailableParentCategories = await _categoryService.GetAllAsync()
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
        /* ESKİ KOMPLİKE METHOD - KULLANIMDA DEĞİL
        // Recursive kategori oluşturma
        private async Task<int?> CreateCategoryRecursive(CategoryTreeNode node, int? parentId)
        {
            // Ana kategoriyi oluştur
            var categoryDto = new CategoryDto
            {
                Name = node.Name,
                Description = node.Description,
                ImageUrl = node.ImageUrl,
                ParentCategoryId = parentId,
                DisplayOrder = node.DisplayOrder,
                IsActive = node.IsActive
            };

            var success = await _categoryService.CreateAsync(categoryDto);
            
            if (!success)
                return null;

            // Oluşturulan kategorinin ID'sini al (son eklenen)
            var categories = await _categoryService.GetAllAsync();
            var createdCategory = categories
                .Where(c => c.Name == node.Name && c.ParentCategoryId == parentId)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefault();

            if (createdCategory == null)
                return null;

        /* ESKİ RECURSIVE METHODLAR - KULLANIMDA DEĞİL
        // Recursive kategori oluşturma
        private async Task<int?> CreateCategoryRecursive(CategoryTreeNode node, int? parentId)
        {
            // Ana kategoriyi oluştur
            var categoryDto = new CategoryDto
            {
                Name = node.Name,
                Description = node.Description,
                ImageUrl = node.ImageUrl,
                ParentCategoryId = parentId,
                DisplayOrder = node.DisplayOrder,
                IsActive = node.IsActive
            };

            var success = await _categoryService.CreateAsync(categoryDto);
            
            if (!success)
                return null;

            // Oluşturulan kategorinin ID'sini al (son eklenen)
            var categories = await _categoryService.GetAllAsync();
            var createdCategory = categories
                .Where(c => c.Name == node.Name && c.ParentCategoryId == parentId)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefault();

            if (createdCategory == null)
                return null;

            // Alt kategorileri oluştur
            if (node.SubCategories != null && node.SubCategories.Any())
            {
                foreach (var subCategory in node.SubCategories)
                {
                    await CreateCategoryRecursive(subCategory, createdCategory.Id);
                }
            }

            return createdCategory.Id;
        }

        // Kategori sayısını hesapla (recursive)
        private int CountCategories(List<CategoryTreeNode> categories)
        {
            int count = categories.Count;
            foreach (var cat in categories)
            {
                if (cat.SubCategories != null && cat.SubCategories.Any())
                {
                    count += CountCategories(cat.SubCategories);
                }
            }
            return count;
        }
        */

        // Düzenleme - GET
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null)
                return NotFound();

            // Alt kategorileri yükle
            var categories = await _categoryService.GetAllAsync();
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
            var success = await _categoryService.DeleteAsync(id);

            if (success)
            {
                TempData["Success"] = "Kategori başarıyla silindi";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Kategori silinirken hata oluştu.");
            var category = await _categoryService.GetByIdAsync(id);
            return View(category);
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

                var success = await _brandService.CreateAsync(brandDto);

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
            
            var models = await _modelService.GetByBrandIdAsync(id);
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

                var success = await _brandService.UpdateAsync(id, brandDto);

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
                var success = await _brandService.DeleteAsync(id);
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
                System.Console.WriteLine($"\n========== CreateModel Başladı ==========");
                System.Console.WriteLine($"BrandId (URL): {brandId}");
                System.Console.WriteLine($"ModelDto alındı: {modelDto != null}");
                
                if (modelDto != null)
                {
                    System.Console.WriteLine($"  Name: {modelDto.Name}");
                    System.Console.WriteLine($"  Description: {modelDto.Description}");
                    System.Console.WriteLine($"  BrandId (DTO): {modelDto.BrandId}");
                    System.Console.WriteLine($"  IsActive: {modelDto.IsActive}");
                }
                
                if (modelDto == null)
                {
                    System.Console.WriteLine("❌ ModelDto null!");
                    return Json(new { success = false, message = "Model bilgileri boş" });
                }
                
                modelDto.BrandId = brandId;
                System.Console.WriteLine($"BrandId set edildi: {modelDto.BrandId}");
                System.Console.WriteLine($"API'ye gönderiliyor...");
                
                var success = await _modelService.CreateModelAsync(modelDto);
                System.Console.WriteLine($"API Sonucu: {success}");
                System.Console.WriteLine($"========== CreateModel Bitti ==========\n");
                
                // AJAX isteği ise sadece JSON döndür, TempData set etme
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || 
                    Request.Headers["Accept"].ToString().Contains("application/json"))
                {
                    return Json(new { success, message = success ? "Model başarıyla eklendi." : "Model eklenirken hata oluştu." });
                }
                
                // Normal form POST için TempData kullan
                TempData[success ? "Success" : "Error"] = success
                    ? "Model başarıyla eklendi."
                    : "Model eklenirken hata oluştu.";
                return RedirectToAction(nameof(Brands));
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"❌ Exception: {ex.Message}");
                System.Console.WriteLine($"Stack: {ex.StackTrace}");
                System.Console.WriteLine($"========== CreateModel Hata ==========\n");
                
                // AJAX isteği ise sadece JSON döndür, TempData set etme
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || 
                    Request.Headers["Accept"].ToString().Contains("application/json"))
                {
                    return Json(new { success = false, message = $"Hata: {ex.Message}" });
                }
                
                // Normal form POST için TempData kullan
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
                System.Console.WriteLine($"\n========== DeleteModel Başladı ==========");
                System.Console.WriteLine($"Model ID: {id}");
                
                await _modelService.DeleteAsync(id);
                System.Console.WriteLine($"Model başarıyla silindi");
                System.Console.WriteLine($"========== DeleteModel Bitti ==========\n");
                
                // AJAX isteği ise sadece JSON döndür, TempData set etme
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || 
                    Request.Headers["Accept"].ToString().Contains("application/json"))
                {
                    return Json(new { success = true, message = "Model başarıyla silindi." });
                }
                
                // Normal form POST için TempData kullan
                TempData["Success"] = "Model başarıyla silindi.";
                return RedirectToAction(nameof(Brands));
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"❌ Exception: {ex.Message}");
                System.Console.WriteLine($"Stack: {ex.StackTrace}");
                System.Console.WriteLine($"========== DeleteModel Hata ==========\n");
                
                // AJAX isteği ise sadece JSON döndür, TempData set etme
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || 
                    Request.Headers["Accept"].ToString().Contains("application/json"))
                {
                    return Json(new { success = false, message = $"Hata: {ex.Message}" });
                }
                
                // Normal form POST için TempData kullan
                TempData["Error"] = $"Hata: {ex.Message}";
                return RedirectToAction(nameof(Brands));
            }
        }

        // Özellik Yönetimi
        public async Task<IActionResult> Attributes()
        {
            var attributes = await _globalAttributeService.GetAllAsync();
            return View(attributes);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAttribute(GlobalAttributeCreateDto dto)
        {
            dto.Values = dto.Values.Where(v => !string.IsNullOrWhiteSpace(v.Value)).ToList();
            var success = await _globalAttributeService.CreateAsync(dto);
            TempData[success ? "Success" : "Error"] = success ? "Özellik eklendi." : "Özellik eklenemedi.";
            return RedirectToAction(nameof(Attributes));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAttribute(int id)
        {
            var success = await _globalAttributeService.DeleteAsync(id);
            TempData[success ? "Success" : "Error"] = success ? "Özellik silindi." : "Özellik silinemedi.";
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
