using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.DTOs;
using Dashboard.Web.Services;

namespace Dashboard.Web.Controllers
{
    [Authorize(Roles = "CompanyAdmin,SuperAdmin,User")]
    public class ProductController : Controller
    {
        private readonly IApiService<ProductViewModel> _productService;
        private readonly IApiService<CategoryViewModel> _categoryService;
        private readonly IApiService<BrandDto> _brandService;
        private readonly IApiService<CompanyDto> _companyService;

        public ProductController(
            IApiService<ProductViewModel> productService,
            IApiService<CategoryViewModel> categoryService,
            IApiService<BrandDto> brandService,
            IApiService<CompanyDto> companyService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _brandService = brandService;
            _companyService = companyService;
        }

        public async Task<IActionResult> Index()
        {
            // SuperAdmin değilse direkt ürün listesine yönlendir
            if (!User.IsInRole("SuperAdmin"))
            {
                return RedirectToAction("List");
            }
            
            // Envanter yönetimi ana sayfası - sadece SuperAdmin için
            return View();
        }

        // Ürün listesi
        public async Task<IActionResult> List()
        {
            var response = await _productService.GetAllAsync();
            if (response == null || response.Data == null)
                return View(new List<ProductViewModel>());
            return View(response.Data);
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
            var categories = await _categoryService.GetAllAsync();
            var brands = await _brandService.GetAllAsync();
        
            ViewBag.Categories = categories;
            ViewBag.Brands = brands;
        
            if (User.IsInRole("SuperAdmin"))
            {
                var companies = await _companyService.GetAllAsync();
                ViewBag.Companies = companies;
            }
        
            return View();
        }

        // POST: Yeni ürün ekleme
        [Authorize(Roles = "CompanyAdmin,SuperAdmin,User")]
        [HttpPost]
        public async Task<IActionResult> Create(ProductCreateDto product)
        {
            // CompanyId kontrolü - eğer set edilmemişse claim'den al
            if (product.CompanyId == 0)
            {
                var companyIdClaim = User.FindFirst("CompanyId")?.Value;
                if (int.TryParse(companyIdClaim, out var companyId))
                {
                    product.CompanyId = companyId;
                }
            }

            Console.WriteLine($"[ProductController] Creating product: {product.Name}, CompanyId: {product.CompanyId}");

            if (!ModelState.IsValid)
            {
                var categories = await _categoryService.GetAllAsync();
                var brands = await _brandService.GetAllAsync();
                ViewBag.Categories = categories;
                ViewBag.Brands = brands;
                if (User.IsInRole("SuperAdmin"))
                {
                    var companies = await _companyService.GetAllAsync();
                    ViewBag.Companies = companies;
                }
                return View(product);
            }

            var success = await _productService.CreateAsync<ProductCreateDto>(product);
            if (success)
            {
                TempData["SuccessMessage"] = "Ürün başarıyla eklendi.";
                return RedirectToAction(nameof(List));
            }

            ModelState.AddModelError("", "Ürün eklenirken hata oluştu.");
            var cats = await _categoryService.GetAllAsync();
            var brds = await _brandService.GetAllAsync();
            ViewBag.Categories = cats;
            ViewBag.Brands = brds;
            if (User.IsInRole("SuperAdmin"))
            {
                var comps = await _companyService.GetAllAsync();
                ViewBag.Companies = comps;
            }
            return View(product);
        }

        // GET: Edit formu
        [Authorize(Roles = "CompanyAdmin,SuperAdmin,User")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
                return NotFound();

            return View(product);
        }

        // POST: Ürün güncelleme
        [Authorize(Roles = "CompanyAdmin,SuperAdmin,User")]
        [HttpPost]
        public async Task<IActionResult> Edit(ProductDto product)
        {
            if (!ModelState.IsValid)
                return View(product);

            var success = await _productService.UpdateAsync(product.Id, product);
            if (success)
                return RedirectToAction(nameof(List));

            ModelState.AddModelError("", "Ürün güncellenirken hata oluştu.");
            return View(product);
        }
        // GET: Delete onay ekranı
        [Authorize(Roles = "CompanyAdmin,SuperAdmin,User")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
                return NotFound();

            return View(product);
        }

        // POST: Ürün silme
        [Authorize(Roles = "CompanyAdmin,SuperAdmin,User")]
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var success = await _productService.DeleteAsync(id);
            if (success)
                return RedirectToAction(nameof(List));

            ModelState.AddModelError("", "Ürün silinirken hata oluştu.");
            var product = await _productService.GetByIdAsync(id);
            return View(product);
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
