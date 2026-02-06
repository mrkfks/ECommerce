using Dashboard.Web.Models;
using Dashboard.Web.Services;
using ECommerce.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dashboard.Web.Controllers
{
    [Authorize(Roles = "CompanyAdmin,SuperAdmin,User")]
    public class ProductController : Controller
    {
        private readonly IApiService<ProductViewModel> _productService;
        private readonly IApiService<ECommerce.Application.DTOs.CategoryDto> _categoryService;
        private readonly IApiService<ECommerce.Application.DTOs.BrandDto> _brandService;
        private readonly IApiService<ECommerce.Application.DTOs.CompanyDto> _companyService;

        public ProductController(
            IApiService<ProductViewModel> productService,
            IApiService<ECommerce.Application.DTOs.CategoryDto> categoryService,
            IApiService<ECommerce.Application.DTOs.BrandDto> brandService,
            IApiService<ECommerce.Application.DTOs.CompanyDto> companyService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _brandService = brandService;
            _companyService = companyService;
        }

        public async Task<IActionResult> Index()
        {
            // Tüm kullanıcıları ürün listesine yönlendir
            return RedirectToAction("List");
        }

        // Ürün listesi
        public async Task<IActionResult> List()
        {
            Console.WriteLine("[ProductController.List] Starting...");
            var response = await _productService.GetPagedListAsync(1, 100); // İlk 100 ürünü getir
            Console.WriteLine($"[ProductController.List] Response: Items count={response?.Items?.Count() ?? 0}");

            if (response == null || response.Items == null)
            {
                Console.WriteLine("[ProductController.List] Response or Items is NULL - returning empty list");
                return View(new List<ProductViewModel>());
            }

            var itemsList = response.Items.ToList();
            Console.WriteLine($"[ProductController.List] Sending {itemsList.Count} items to view");
            return View(itemsList);
        }

        // GET: Ürün detayları
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var response = await _productService.GetByIdAsync(id);
            if (response == null || response.Data == null)
                return NotFound();
            return View(response.Data);
        }

        // GET: Create formu
        [Authorize(Roles = "CompanyAdmin,SuperAdmin,User")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            Console.WriteLine("[ProductController.Create] Starting...");

            var categories = await _categoryService.GetAllAsync();
            Console.WriteLine($"[ProductController.Create] Categories response: Success={categories?.Success}, Data count={categories?.Data?.Count()}");

            var brands = await _brandService.GetAllAsync();
            Console.WriteLine($"[ProductController.Create] Brands response: Success={brands?.Success}, Data count={brands?.Data?.Count()}");

            // API CategoryDto döndürüyor, CategoryViewModel değil
            var categoryDtos = categories?.Data?.Select(c => new ECommerce.Application.DTOs.CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ImageUrl = c.ImageUrl,
                ParentCategoryId = c.ParentCategoryId,
                IsActive = c.IsActive
            }).ToList() ?? new List<ECommerce.Application.DTOs.CategoryDto>();

            var categoryList = categoryDtos.Where(x => x.Id != 0 && !string.IsNullOrEmpty(x.Name)).ToList();
            var brandList = (brands?.Data ?? new List<ECommerce.Application.DTOs.BrandDto>()).Where(x => x.Id != 0 && !string.IsNullOrEmpty(x.Name)).ToList();

            Console.WriteLine($"[ProductController.Create] Filtered Categories count: {categoryList.Count}");
            Console.WriteLine($"[ProductController.Create] Filtered Brands count: {brandList.Count}");

            ViewBag.Categories = categoryList;
            ViewBag.Brands = brandList;

            if (User.IsInRole("SuperAdmin"))
            {
                Console.WriteLine("[ProductController.Create] User is SuperAdmin, fetching companies...");
                var companies = await _companyService.GetAllAsync();
                Console.WriteLine($"[ProductController.Create] Companies response: Success={companies?.Success}, Data count={companies?.Data?.Count()}");

                var companyList = (companies?.Data ?? new List<ECommerce.Application.DTOs.CompanyDto>()).Where(x => x.Id != 0 && !string.IsNullOrEmpty(x.Name)).ToList();
                Console.WriteLine($"[ProductController.Create] Filtered Companies count: {companyList.Count}");

                ViewBag.Companies = companyList;
            }

            return View();
        }

        // POST: Yeni ürün ekleme
        [Authorize(Roles = "CompanyAdmin,SuperAdmin,User")]
        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> Create(ProductCreateDto product, IFormFile? imageFile)
        {
            if (product == null)
            {
                Console.WriteLine("[ProductController] Product is null.");
                return BadRequest("Product is null.");
            }

            if (string.IsNullOrEmpty(product.Name) || product.CompanyId == 0)
            {
                Console.WriteLine("[ProductController] Product details are invalid.");
                return BadRequest("Product details are invalid.");
            }

            // CompanyId kontrolü - eğer set edilmemişse claim'den al
            if (product != null && product.CompanyId == 0)
            {
                var companyIdClaim = User.FindFirst("CompanyId")?.Value;
                if (!string.IsNullOrEmpty(companyIdClaim) && int.TryParse(companyIdClaim, out var companyId))
                {
                    product.CompanyId = companyId;
                }
            }

            // Eğer dosya seçildiyse önce resmi API'ye yükle
            if (imageFile != null && imageFile.Length > 0)
            {
                using var ms = new MemoryStream();
                await imageFile.CopyToAsync(ms);
                ms.Position = 0;
                var content = new MultipartFormDataContent();
                var fileContent = new ByteArrayContent(ms.ToArray());
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(imageFile.ContentType);
                content.Add(fileContent, "file", imageFile.FileName);

                // API dosya upload endpointi
                var apiBaseUrl = Environment.GetEnvironmentVariable("ApiBaseUrl") ?? "http://localhost:5010";
                var uploadUrl = $"{apiBaseUrl}/api/fileupload/product";
                using var httpClient = new HttpClient();
                var uploadResponse = await httpClient.PostAsync(uploadUrl, content);
                if (uploadResponse.IsSuccessStatusCode)
                {
                    var json = await uploadResponse.Content.ReadAsStringAsync();
                    var result = System.Text.Json.JsonSerializer.Deserialize<ECommerce.Application.DTOs.ImageUploadResultDto>(json);
                    if (product != null && result != null && !string.IsNullOrEmpty(result.OriginalUrl))
                    {
                        product.ImageUrl = result.OriginalUrl;
                    }
                }
            }

            if (product != null)
            {
                var productViewModel = new Dashboard.Web.Models.ProductViewModel
                {
                    Name = product.Name,
                    Description = product.Description ?? string.Empty,
                    Price = product.Price,
                    StockQuantity = product.StockQuantity,
                    CategoryId = product.CategoryId,
                    BrandId = product.BrandId,
                    CompanyId = product.CompanyId,
                    ModelId = product.ModelId,
                    ImageUrl = product.ImageUrl ?? string.Empty,
                    IsActive = product.IsActive,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                var createResponse = await _productService.CreateAsync(productViewModel);
                if (createResponse.Success)
                {
                    TempData["SuccessMessage"] = "Ürün başarıyla eklendi.";
                    return RedirectToAction(nameof(List));
                }

                ModelState.AddModelError("", $"Ürün eklenirken hata oluştu: {createResponse.Message}");
                var cats = await _categoryService.GetAllAsync();
                var brds = await _brandService.GetAllAsync();
                ViewBag.Categories = (cats?.Data ?? new List<ECommerce.Application.DTOs.CategoryDto>()).Where(x => x.Id != 0 && !string.IsNullOrEmpty(x.Name)).ToList();
                ViewBag.Brands = (brds?.Data ?? new List<ECommerce.Application.DTOs.BrandDto>()).Where(x => x.Id != 0 && !string.IsNullOrEmpty(x.Name)).ToList();
                if (User.IsInRole("SuperAdmin"))
                {
                    var companies = await _companyService.GetAllAsync();
                    ViewBag.Companies = (companies?.Data ?? new List<ECommerce.Application.DTOs.CompanyDto>()).Where(x => x.Id != 0 && !string.IsNullOrEmpty(x.Name)).ToList();
                }

                return View(productViewModel);
            }
            else
            {
                return View("Error"); // Hata sayfasına yönlendirme
            }
        }

        // GET: Edit formu
        [Authorize(Roles = "CompanyAdmin,SuperAdmin,User")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            Console.WriteLine($"[ProductController.Edit] Loading product with id: {id}");
            var response = await _productService.GetByIdAsync(id);
            Console.WriteLine($"[ProductController.Edit] Response: Success={response?.Success}, Data={response?.Data?.Name}");

            if (response == null || response.Data == null)
            {
                Console.WriteLine("[ProductController.Edit] Product not found!");
                return NotFound();
            }

            // Dropdown'lar için kategorileri ve markaları yükle
            var categories = await _categoryService.GetAllAsync();
            var brands = await _brandService.GetAllAsync();

            ViewBag.Categories = (categories?.Data ?? new List<ECommerce.Application.DTOs.CategoryDto>()).Where(x => x.Id != 0 && !string.IsNullOrEmpty(x.Name)).ToList();
            ViewBag.Brands = (brands?.Data ?? new List<ECommerce.Application.DTOs.BrandDto>()).Where(x => x.Id != 0 && !string.IsNullOrEmpty(x.Name)).ToList();

            Console.WriteLine($"[ProductController.Edit] Returning view with product: {response.Data.Name}");
            return View(response.Data);
        }

        // POST: Ürün güncelleme
        [Authorize(Roles = "CompanyAdmin,SuperAdmin,User")]
        [HttpPost]
        public async Task<IActionResult> Edit(ProductViewModel product)
        {
            if (!ModelState.IsValid)
                return View(product);

            // ProductViewModel'i ProductFormDto'ya dönüştür
            var productFormDto = new ProductFormDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                CategoryId = product.CategoryId,
                BrandId = product.BrandId,
                ModelId = product.ModelId,
                ImageUrl = product.ImageUrl,
                IsActive = product.IsActive
            };

            var response = await _productService.UpdateAsync<ProductFormDto>(product.Id, productFormDto);
            if (response)
                return RedirectToAction(nameof(List));

            ModelState.AddModelError("", "Ürün güncellenirken hata oluştu.");
            return View(product);
        }
        // GET: Delete onay ekranı
        [Authorize(Roles = "CompanyAdmin,SuperAdmin,User")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _productService.GetByIdAsync(id);
            if (response == null || response.Data == null)
                return NotFound();

            return View(response.Data);
        }

        // POST: Ürün silme
        [Authorize(Roles = "CompanyAdmin,SuperAdmin,User")]
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _productService.DeleteAsync(id);
            if (result.Success)
                return RedirectToAction(nameof(List));

            ModelState.AddModelError("", "Ürün silinirken hata oluştu.");
            var response = await _productService.GetByIdAsync(id);
            return View(response?.Data);
        }

        [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
        [HttpPost]
        public async Task<IActionResult> BulkUpdate([FromBody] ProductBulkUpdateDto dto)
        {
            // var success = await _productService.BulkUpdateAsync(dto.ProductIds, dto.PriceIncreasePercentage);
            var success = await _productService.PostActionAsync("bulk-price-update", dto);
            return Json(new { success = success, message = success ? "Fiyatlar güncellendi" : "Hata oluştu" });
        }
    }
}
