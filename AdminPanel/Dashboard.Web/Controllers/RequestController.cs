using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.DTOs;
using Dashboard.Web.Services;

namespace Dashboard.Web.Controllers
{
    public class RequestController : Controller
    {
        private readonly IApiService<RequestDto> _requestService;

        public RequestController(IApiService<RequestDto> requestService)
        {
            _requestService = requestService;
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
                var success = await _requestService.CreateAsync<RequestCreateDto>(dto);

                if (success)
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



        [Authorize]
        public async Task<IActionResult> MyRequests()
        {
            try
            {
                var companyIdClaim = User.FindFirst("CompanyId")?.Value;
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out int companyId))
                {
                    return BadRequest("Sirket bilgisi alinamadi");
                }

                var allRequests = await _requestService.GetAllAsync();
                var requests = allRequests?.Where(r => r.CompanyId == companyId).ToList() ?? new List<RequestDto>();
                return View(requests);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Talepler yüklenirken hata oluştu: {ex.Message}";
                return View(new List<RequestDto>());
            }
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            if (!User.IsInRole("SuperAdmin"))
            {
                return RedirectToAction(nameof(MyRequests));
            }

            var requests = await _requestService.GetAllAsync();
            return View(requests);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var request = await _requestService.GetByIdAsync(id);

            if (request == null)
                return NotFound();

            // SuperAdmin ise tum talepləri gorebilir
            if (User.IsInRole("SuperAdmin"))
                return View(request);

            // Normal user sadece kendi şirketinin taleplerini gorebilir
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out int companyId))
                return Forbid();

            if (request.CompanyId != companyId)
                return Forbid();

            return View(request);
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        public async Task<IActionResult> Approve(int id, string? feedback)
        {
            var success = await _requestService.PostActionAsync($"{id}/approve", new { });

            if (success)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Talep onaylanırken hata oluştu.");
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        public async Task<IActionResult> Reject(int id, string? feedback)
        {
            var success = await _requestService.PostActionAsync($"{id}/reject", new { });

            if (success)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Talep reddedilirken hata oluştu.");
            return RedirectToAction(nameof(Index));
        }
    }
}
