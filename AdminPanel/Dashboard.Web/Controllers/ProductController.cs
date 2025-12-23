using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.DTOs;
using Dashboard.Web.Services;

namespace Dashboard.Web.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly ProductApiService _productService;
        private readonly CategoryApiService _categoryService;
        private readonly BrandApiService _brandService;
        private readonly CompanyApiService _companyService;

        public ProductController(
            ProductApiService productService,
            CategoryApiService categoryService,
            BrandApiService brandService,
            CompanyApiService companyService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _brandService = brandService;
            _companyService = companyService;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllAsync();
            return View(products);
        }

        // GET: Ürün detayları
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
                return NotFound();

            return View(product);
        }

        // GET: Create formu
        [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
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
        [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
        [HttpPost]
        public async Task<IActionResult> Create(ProductDto product)
        {
            if (!ModelState.IsValid)
                return View(product);

            var success = await _productService.CreateAsync(product);
            if (success)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Ürün eklenirken hata oluştu.");
            return View(product);
        }

        // GET: Edit formu
        [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
                return NotFound();

            return View(product);
        }

        // POST: Ürün güncelleme
        [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
        [HttpPost]
        public async Task<IActionResult> Edit(ProductDto product)
        {
            if (!ModelState.IsValid)
                return View(product);

            var success = await _productService.UpdateAsync(product.Id, product);
            if (success)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Ürün güncellenirken hata oluştu.");
            return View(product);
        }
        // GET: Delete onay ekranı
        [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
                return NotFound();

            return View(product);
        }

        // POST: Ürün silme
        [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var success = await _productService.DeleteAsync(id);
            if (success)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Ürün silinirken hata oluştu.");
            var product = await _productService.GetByIdAsync(id);
            return View(product);
        }
    }
}
