using System.Text.Json;
using Dashboard.Web.Models;
using Dashboard.Web.Services;
using ECommerce.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GlobalAttributeDto = ECommerce.Application.DTOs.GlobalAttributeDto;

namespace Dashboard.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class CategoryController : Controller
    {
        private readonly IApiService<CategoryFormDto> _categoryService;
        private readonly IApiService<ECommerce.Application.DTOs.BrandDto> _brandService;
        private readonly IApiService<Dashboard.Web.Models.ModelDto> _modelService;
        private readonly IApiService<GlobalAttributeDto> _globalAttributeService;

        public CategoryController(
            IApiService<CategoryFormDto> categoryService,
            IApiService<ECommerce.Application.DTOs.BrandDto> brandService,
            IApiService<Dashboard.Web.Models.ModelDto> modelService,
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
                return View(new List<CategoryDto>());
            var model = response.Data
                .Where(x => x.Id.HasValue && x.Id.Value > 0)
                .Select(x => new CategoryDto
                {
                    Id = x.Id.Value,
                    Name = x.Name,
                    Description = x.Description ?? string.Empty,
                    ParentCategoryId = x.ParentCategoryId,
                    ImageUrl = x.ImageUrl,
                    IsActive = x.IsActive,
                    DisplayOrder = x.DisplayOrder
                }).ToList();
            if (!model.Any())
            {
                TempData["Error"] = "Kategori listesi boş veya hatalı veri döndü. Lütfen API ve veritabanı bağlantısını kontrol edin.";
            }
            return View(model);
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
            var categories = categoriesResponse?.Data?.Select(x => new CategoryDto
            {
                Id = x.Id ?? 0,
                Name = x.Name,
                Description = x.Description ?? string.Empty,
                ParentCategoryId = x.ParentCategoryId,
                ImageUrl = x.ImageUrl,
                IsActive = x.IsActive,
                DisplayOrder = x.DisplayOrder
            }).ToList() ?? new List<CategoryDto>();
            var viewModel = new CategoryViewModel
            {
                AvailableParentCategories = categories.ToList()
            };
            return View(viewModel);
        }

        // Ana kategori ekleme - POST (Basit)
        [HttpPost]
        public async Task<IActionResult> CreateCategory(CategoryFormDto categoryDto, IFormFile? ImageFile)
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

                    categoryDto = new CategoryFormDto
                    {
                        Id = categoryDto.Id ?? 0,
                        Name = categoryDto.Name,
                        Description = categoryDto.Description ?? string.Empty,
                        ParentCategoryId = null,
                        ImageUrl = $"/uploads/categories/{fileName}",
                        IsActive = categoryDto.IsActive,
                        DisplayOrder = categoryDto.DisplayOrder
                    };
                }
                var response = await _categoryService.CreateAsync(categoryDto);
                if (response != null && response.Success)
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
        public async Task<IActionResult> CreateSubCategory(CategoryFormDto categoryDto, IFormFile? ImageFile)
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

                    categoryDto = new CategoryFormDto
                    {
                        Id = categoryDto.Id ?? 0,
                        Name = categoryDto.Name,
                        Description = categoryDto.Description ?? string.Empty,
                        ParentCategoryId = categoryDto.ParentCategoryId,
                        ImageUrl = $"/uploads/categories/{fileName}",
                        IsActive = categoryDto.IsActive,
                        DisplayOrder = categoryDto.DisplayOrder
                    };
                }

                if (!categoryDto.ParentCategoryId.HasValue)
                {
                    TempData["Error"] = "Üst kategori seçilmelidir.";
                    return RedirectToAction(nameof(Index));
                }

                var response = await _categoryService.CreateAsync(categoryDto);
                if (response != null && response.Success)
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
                return RedirectToAction(nameof(Index));
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
                return RedirectToAction(nameof(Index));
            }
        }

        // Özellik Yönetimi
        [HttpGet]
        public async Task<IActionResult> Attributes()
        {
            var response = await _globalAttributeService.GetAllAsync();
            var attributes = response?.Data ?? new List<ECommerce.Application.DTOs.GlobalAttributeDto>();
            return View(attributes);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAttribute(GlobalAttributeCreateDto dto)
        {
            // Values null ise boş liste kullan, değilse filtrele
            dto.Values = dto.Values?.Where(v => !string.IsNullOrWhiteSpace(v.Value)).ToList() ?? new List<Dashboard.Web.Models.GlobalAttributeValueFormDto>();

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

        [HttpGet]
        public async Task<IActionResult> Brands()
        {
            var response = await _brandService.GetAllAsync();
            var brands = response?.Data ?? new List<ECommerce.Application.DTOs.BrandDto>();
            return View(brands);
        }
    }
}
