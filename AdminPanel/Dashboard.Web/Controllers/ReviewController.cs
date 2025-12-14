using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using ECommerce.Application.DTOs;

namespace Dashboard.Web.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ReviewController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Listeleme
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var reviews = await client.GetFromJsonAsync<List<ReviewDto>>("api/Review");
            return View(reviews ?? new List<ReviewDto>());
        }

        // Detay
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var review = await client.GetFromJsonAsync<ReviewDto>($"api/Review/{id}");

            if (review == null)
                return NotFound();

            return View(review);
        }

        // Silme
        [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var review = await client.GetFromJsonAsync<ReviewDto>($"api/Review/{id}");

            if (review == null)
                return NotFound();

            return View(review);
        }

        [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var response = await client.DeleteAsync($"api/Review/{id}");

            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Yorum silinirken hata oluştu.");
            var review = await client.GetFromJsonAsync<ReviewDto>($"api/Review/{id}");
            return View(review);
        }
    }
}

