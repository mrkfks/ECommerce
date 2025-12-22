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

        // Talep oluşturma formu (SuperAdmin hariç tüm kullanıcılar)
        [Authorize]
        [HttpGet]
        public IActionResult Create()
        {
            // SuperAdmin talep oluşturamaz
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

        // Talep gönderme (SuperAdmin hariç tüm kullanıcılar)
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
                    return RedirectToAction("MyRequests");
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

        // Liste: SuperAdmin tüm talepler, diğer roller kendi taleplerine yönlendirilir
        [Authorize]
        public async Task<IActionResult> Index()
        {
            if (!User.IsInRole("SuperAdmin"))
            {
                return RedirectToAction(nameof(MyRequests));
            }

            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var requests = await client.GetFromJsonAsync<List<RequestDto>>("api/Request") ?? new List<RequestDto>();
            return View(requests);
        }

        // Detay: SuperAdmin tüm talepləri görebilir, users sadece kendi taleplerini görebilir
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var request = await client.GetFromJsonAsync<RequestDto>($"api/Request/{id}");

            if (request == null)
                return NotFound();

            // SuperAdmin ise tüm talepləri görebilir
            if (User.IsInRole("SuperAdmin"))
                return View(request);

            // Normal user sadece kendi şirketinin taleplerini görebilir
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out int companyId))
                return Forbid();

            if (request.CompanyId != companyId)
                return Forbid();

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
