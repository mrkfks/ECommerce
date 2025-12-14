using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using ECommerce.Application.DTOs;

namespace Dashboard.Web.Controllers
{
    public class RequestController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public RequestController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Talep oluşturma formu (Tüm authorized users)
        [Authorize]
        [HttpGet]
        public IActionResult Create()
        {
            // SuperAdmin talepleri oluşturmaz; sadece yanıtlar
            if (User.IsInRole("SuperAdmin"))
            {
                return Forbid();
            }

            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out int companyId))
            {
                return BadRequest("Şirket bilgisi alınamadı");
            }

            return View(new RequestCreateDto { CompanyId = companyId });
        }

        // Talep gönderme (Tüm authorized users)
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(RequestCreateDto dto)
        {
            if (User.IsInRole("SuperAdmin"))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                var companyIdClaim = User.FindFirst("CompanyId")?.Value;
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out int companyId))
                {
                    ModelState.AddModelError("", "Şirket bilgisi alınamadı");
                    return View(dto);
                }

                dto.CompanyId = companyId;
                var client = _httpClientFactory.CreateClient("ECommerceApi");
                var response = await client.PostAsJsonAsync("api/Request", dto);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Talep başarıyla gönderildi!";
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Talep gönderilirken hata oluştu.");
                return View(dto);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Hata: {ex.Message}");
                return View(dto);
            }
        }

        // Kullanıcı: Kendi şirketinin taleplerini görebilsin (Taleplerim)
        [Authorize]
        public async Task<IActionResult> MyRequests()
        {
            try
            {
                // Kullanıcının şirket ID'sini claims'ten al
                var companyIdClaim = User.FindFirst("CompanyId")?.Value;
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out int companyId))
                {
                    return BadRequest("Şirket bilgisi alınamadı");
                }

                var client = _httpClientFactory.CreateClient("ECommerceApi");
                var requests = await client.GetFromJsonAsync<List<RequestDto>>($"api/Request/company/{companyId}") ?? new List<RequestDto>();
                return View(requests);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Talepler yüklenirken hata oluştu: {ex.Message}";
                return View(new List<RequestDto>());
            }
        }

        // SuperAdmin: Talepler listesi
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var requests = await client.GetFromJsonAsync<List<RequestDto>>("api/Request") ?? new List<RequestDto>();
            return View(requests);
        }

        // SuperAdmin: Talep detayı
        [Authorize(Roles = "SuperAdmin")]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var request = await client.GetFromJsonAsync<RequestDto>($"api/Request/{id}");

            if (request == null)
                return NotFound();

            return View(request);
        }

        // SuperAdmin: Talep onayı
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        public async Task<IActionResult> Approve(int id, string? feedback)
        {
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var response = await client.PostAsJsonAsync($"api/Request/{id}/approve", new { feedback });

            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Talep onaylanırken hata oluştu.");
            return RedirectToAction(nameof(Index));
        }

        // SuperAdmin: Talep reddi
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        public async Task<IActionResult> Reject(int id, string? feedback)
        {
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var response = await client.PostAsJsonAsync($"api/Request/{id}/reject", new { feedback });

            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Talep reddedilirken hata oluştu.");
            return RedirectToAction(nameof(Index));
        }
    }
}
