using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using ECommerce.Application.DTOs;

namespace Dashboard.Web.Controllers
{
    public class UserController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public UserController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Listeleme
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var users = await client.GetFromJsonAsync<List<UserDto>>("api/User");
            return View(users ?? new List<UserDto>());
        }

        // Detay
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var user = await client.GetFromJsonAsync<UserDto>($"api/User/{id}");

            if (user == null)
                return NotFound();

            return View(user);
        }

        // Rol atama/düzenleme
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var user = await client.GetFromJsonAsync<UserDto>($"api/User/{id}");

            if (user == null)
                return NotFound();

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UserDto user)
        {
            if (!ModelState.IsValid)
                return View(user);

            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var response = await client.PutAsJsonAsync($"api/User/{user.Id}", user);

            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Kullanıcı güncellenirken hata oluştu.");
            return View(user);
        }

        // Silme
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var user = await client.GetFromJsonAsync<UserDto>($"api/User/{id}");

            if (user == null)
                return NotFound();

            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var response = await client.DeleteAsync($"api/User/{id}");

            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Kullanıcı silinirken hata oluştu.");
            var user = await client.GetFromJsonAsync<UserDto>($"api/User/{id}");
            return View(user);
        }
    }
}
