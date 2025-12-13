using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using ECommerce.Application.DTOs; // Artık Application DTO’larını doğrudan kullanıyoruz

namespace Dashboard.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ProductController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var products = await client.GetFromJsonAsync<List<ProductDto>>("api/Product");
            return View(products ?? new List<ProductDto>());
        }
        // GET: Create formu
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Yeni ürün ekleme
        [HttpPost]
        public async Task<IActionResult> Create(ProductDto product)
        {
            if (!ModelState.IsValid)
                return View(product);

            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var response = await client.PostAsJsonAsync("api/Product", product);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Ürün eklenirken hata oluştu.");
            return View(product);
        }

        // GET: Edit formu
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var product = await client.GetFromJsonAsync<ProductDto>($"api/Product/{id}");

            if (product == null)
                return NotFound();

            return View(product);
        }

        // POST: Ürün güncelleme
        [HttpPost]
        public async Task<IActionResult> Edit(ProductDto product)
        {
            if (!ModelState.IsValid)
                return View(product);

            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var response = await client.PutAsJsonAsync($"api/Product/{product.Id}", product);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Ürün güncellenirken hata oluştu.");
            return View(product);
        }
        // GET: Delete onay ekranı
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var product = await client.GetFromJsonAsync<ProductDto>($"api/Product/{id}");

            if (product == null)
                return NotFound();

            return View(product);
        }

        // POST: Ürün silme
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var response = await client.DeleteAsync($"api/Product/{id}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Ürün silinirken hata oluştu.");
            var product = await client.GetFromJsonAsync<ProductDto>($"api/Product/{id}");
            return View(product);
        }
    }
}
