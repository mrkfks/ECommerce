using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;

namespace Dashboard.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class CompanyController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CompanyController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var companies = await client.GetFromJsonAsync<List<CompanyVm>>("api/company");
            return View(companies ?? new List<CompanyVm>());
        }

        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var resp = await client.PostAsync($"api/company/{id}/approve", null);
            TempData[resp.IsSuccessStatusCode ? "Success" : "Error"] = resp.IsSuccessStatusCode ? "Şirket onaylandı" : "Onay işlemi başarısız";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Deactivate(int id)
        {
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var resp = await client.PostAsync($"api/company/{id}/deactivate", null);
            TempData[resp.IsSuccessStatusCode ? "Success" : "Error"] = resp.IsSuccessStatusCode ? "Şirket pasifleştirildi" : "Pasifleştirme başarısız";
            return RedirectToAction(nameof(Index));
        }
    }

    public class CompanyVm
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string TaxNumber { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsApproved { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
