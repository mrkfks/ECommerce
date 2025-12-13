using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http;
using System.Net.Http.Json;
using ECommerce.Application.DTOs;

namespace Dashboard.Web.Controllers
{
    public class CustomerController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CustomerController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Listeleme
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var customers = await client.GetFromJsonAsync<List<CustomerDto>>("api/Customer");
            return View(customers);
        }

        // Detay
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var customer = await client.GetFromJsonAsync<CustomerDto>($"api/Customer/{id}");

            if (customer == null)
                return NotFound();

            return View(customer);
        }

        // Düzenleme
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var customer = await client.GetFromJsonAsync<CustomerDto>($"api/Customer/{id}");

            if (customer == null)
                return NotFound();

            return View(customer);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Edit(CustomerDto customer)
        {
            if (!ModelState.IsValid)
                return View(customer);

            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var response = await client.PutAsJsonAsync($"api/Customer/{customer.Id}", customer);

            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Müşteri güncellenirken hata oluştu.");
            return View(customer);
        }

        // Silme
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var customer = await client.GetFromJsonAsync<CustomerDto>($"api/Customer/{id}");

            if (customer == null)
                return NotFound();

            return View(customer);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var response = await client.DeleteAsync($"api/Customer/{id}");

            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Müşteri silinirken hata oluştu.");
            var customer = await client.GetFromJsonAsync<CustomerDto>($"api/Customer/{id}");
            return View(customer);
        }
    }
}
