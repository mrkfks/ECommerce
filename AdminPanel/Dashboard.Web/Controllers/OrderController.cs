using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using ECommerce.Application.DTOs;

namespace Dashboard.Web.Controllers
{
    public class OrderController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public OrderController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Listeleme
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var orders = await client.GetFromJsonAsync<List<OrderDto>>("api/Order");
            return View(orders ?? new List<OrderDto>());
        }

        // Detay
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var order = await client.GetFromJsonAsync<OrderDto>($"api/Order/{id}");

            if (order == null)
                return NotFound();

            return View(order);
        }

        // Durum güncelleme (Edit)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var order = await client.GetFromJsonAsync<OrderDto>($"api/Order/{id}");

            if (order == null)
                return NotFound();

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(OrderDto order)
        {
            if (!ModelState.IsValid)
                return View(order);

            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var response = await client.PutAsJsonAsync($"api/Order/{order.Id}", order);

            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Sipariş güncellenirken hata oluştu.");
            return View(order);
        }
    }

}