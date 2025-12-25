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
        private readonly CategoryApiService _categoryService;
        private readonly BrandApiService _brandService;
        private readonly ModelApiService _modelService;

        public CategoryController(
            CategoryApiService categoryService,
            BrandApiService brandService,
            ModelApiService modelService)
        {
            _categoryService = categoryService;
            _brandService = brandService;
            _modelService = modelService;
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

        // Yeni kategori ekleme - POST (Hiyerarşik yapı ile)
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] string categoryData)
        {
            try
            {
                var formData = JsonSerializer.Deserialize<CategoryTreeFormData>(categoryData);
                
                if (formData == null || formData.Categories == null || !formData.Categories.Any())
                {
                    TempData["Error"] = "En az bir kategori eklemelisiniz";
                    return RedirectToAction(nameof(Create));
                }

                // Kategorileri recursive olarak kaydet
                foreach (var category in formData.Categories)
                {
                    await CreateCategoryRecursive(category, null);
                }

                TempData["Success"] = $"{CountCategories(formData.Categories)} kategori başarıyla eklendi";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Hata: {ex.Message}";
                return RedirectToAction(nameof(Create));
            }
        }

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
        public async Task<IActionResult> Edit(CategoryDto category, [FromForm] string newSubCategoriesData)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _categoryService.GetAllAsync();
                ViewBag.SubCategories = categories.Where(c => c.ParentCategoryId == category.Id).ToList();
                return View(category);
            }

            var success = await _categoryService.UpdateAsync(category.Id, category);

            if (success)
            {
                // Yeni alt kategorileri kaydet
                if (!string.IsNullOrEmpty(newSubCategoriesData))
                {
                    try
                    {
                        var newSubCategories = JsonSerializer.Deserialize<List<CategoryTreeNode>>(newSubCategoriesData);
                        if (newSubCategories != null && newSubCategories.Any())
                        {
                            foreach (var subCat in newSubCategories)
                            {
                                await CreateCategoryRecursive(subCat, category.Id);
                            }
                            TempData["Success"] = $"Kategori ve {newSubCategories.Count} alt kategori başarıyla güncellendi";
                        }
                        else
                        {
                            TempData["Success"] = "Kategori başarıyla güncellendi";
                        }
                    }
                    catch (Exception ex)
                    {
                        TempData["Warning"] = $"Kategori güncellendi ancak alt kategoriler eklenirken hata oluştu: {ex.Message}";
                    }
                }
                else
                {
                    TempData["Success"] = "Kategori başarıyla güncellendi";
                }
                
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Kategori güncellenirken hata oluştu.");
            var allCategories = await _categoryService.GetAllAsync();
            ViewBag.SubCategories = allCategories.Where(c => c.ParentCategoryId == category.Id).ToList();
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
