using Dashboard.Web.Models;
using Dashboard.Web.Services;
using ECommerce.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dashboard.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class CompanyController : Controller
    {
        private readonly IApiService<CompanyDto> _companyService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CompanyController> _logger;

        public CompanyController(IApiService<CompanyDto> companyService, IHttpClientFactory httpClientFactory, ILogger<CompanyController> logger)
        {
            _companyService = companyService;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("[CompanyController.Index] START - Fetching companies...");
            var companies = await _companyService.GetAllAsync();
            _logger.LogInformation("[CompanyController.Index] API Response - Success: {Success}, Message: {Message}, Data is null: {IsNull}, Count: {Count}",
                companies?.Success, companies?.Message, companies?.Data == null, companies?.Data?.Count() ?? 0);

            if (companies?.Data != null)
            {
                foreach (var c in companies.Data)
                {
                    _logger.LogInformation("[CompanyController.Index] Company: Id={Id}, Name={Name}, Email={Email}", c.Id, c.Name, c.Email);
                }
            }

            var companyVms = (companies?.Data ?? new List<CompanyDto>()).Select(c => new CompanyVm
            {
                Id = c.Id,
                Name = c.Name,
                Address = c.Address,
                PhoneNumber = c.PhoneNumber,
                Email = c.Email,
                TaxNumber = c.TaxNumber,
                ResponsiblePersonName = c.ResponsiblePersonName,
                ResponsiblePersonPhone = c.ResponsiblePersonPhone,
                ResponsiblePersonEmail = c.ResponsiblePersonEmail,
                IsActive = c.IsActive,
                IsApproved = c.IsApproved,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            }).ToList() ?? new List<CompanyVm>();
            return View(companyVms);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CompanyFormDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CompanyFormDto dto)
        {
            _logger.LogInformation("[CompanyController.Create] Starting - Name: {Name}, Email: {Email}", dto.Name, dto.Email);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("[CompanyController.Create] ModelState is invalid");
                return View(dto);
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient("CompanyApi");
                _logger.LogInformation("[CompanyController.Create] Sending POST request to /api/companies");

                var response = await httpClient.PostAsJsonAsync("/api/companies", dto);

                _logger.LogInformation("[CompanyController.Create] Response: {StatusCode}", response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("[CompanyController.Create] Success - Response: {Content}", content);
                    TempData["Success"] = "Şirket başarıyla kaydedildi";
                    return RedirectToAction(nameof(Index));
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("[CompanyController.Create] Failed - Status: {StatusCode}, Error: {Error}", response.StatusCode, errorContent);
                ModelState.AddModelError("", $"Şirket kaydedilirken hata oluştu: {response.StatusCode} - {errorContent}");
                return View(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CompanyController.Create] Exception occurred");
                ModelState.AddModelError("", $"Şirket kaydedilirken hata oluştu: {ex.Message}");
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var company = await _companyService.GetByIdAsync(id);
            if (company == null)
                return NotFound();

            var companyVm = new CompanyVm
            {
                Id = company.Data?.Id ?? 0,
                Name = company.Data?.Name ?? string.Empty,
                Address = company.Data?.Address ?? string.Empty,
                PhoneNumber = company.Data?.PhoneNumber ?? string.Empty,
                Email = company.Data?.Email ?? string.Empty,
                TaxNumber = company.Data?.TaxNumber ?? string.Empty,
                ResponsiblePersonName = company.Data?.ResponsiblePersonName,
                ResponsiblePersonPhone = company.Data?.ResponsiblePersonPhone,
                ResponsiblePersonEmail = company.Data?.ResponsiblePersonEmail,
                IsActive = company.Data?.IsActive ?? false,
                IsApproved = company.Data?.IsApproved ?? false,
                CreatedAt = company.Data?.CreatedAt,
                UpdatedAt = company.Data?.UpdatedAt,
                Domain = company.Data?.Domain,
                LogoUrl = company.Data?.LogoUrl,
                PrimaryColor = company.Data?.PrimaryColor,
                SecondaryColor = company.Data?.SecondaryColor
            };
            return View(companyVm);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var response = await _companyService.GetByIdAsync(id);
            if (response?.Data == null)
                return NotFound();

            return View(response.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, CompanyDto company)
        {
            if (id != company.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(company);

            var response = await _companyService.UpdateAsync(id, company);
            if (response != null && response.Success)
            {
                TempData["Success"] = "Şirket bilgileri başarıyla güncellendi";
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("", "Şirket güncellenirken hata oluştu");
            return View(company);
        }

        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var success = await _companyService.PostActionAsync($"{id}/approve", new { });
            TempData[success ? "Success" : "Error"] = success ? "Şirket onaylandı" : "Onay işlemi başarısız";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Deactivate(int id)
        {
            var success = await _companyService.PostActionAsync($"{id}/deactivate", new { });
            TempData[success ? "Success" : "Error"] = success ? "Şirket pasifleştirildi" : "Pasifleştirme başarısız";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Activate(int id)
        {
            var success = await _companyService.PostActionAsync($"{id}/activate", new { });
            TempData[success ? "Success" : "Error"] = success ? "Şirket aktif hale getirildi" : "Aktivasyon başarısız";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Branding(int id)
        {
            var response = await _companyService.GetByIdAsync(id);
            if (response?.Data == null)
                return NotFound();

            var brandingVm = new CompanyBrandingVm
            {
                Id = response.Data.Id,
                Name = response.Data.Name,
                Domain = response.Data.Domain,
                LogoUrl = response.Data.LogoUrl,
                PrimaryColor = response.Data.PrimaryColor ?? "#3b82f6",
                SecondaryColor = response.Data.SecondaryColor ?? "#1e40af"
            };

            return View(brandingVm);
        }

        [HttpPost]
        public async Task<IActionResult> Branding(int id, CompanyBrandingVm vm)
        {
            if (id != vm.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                var httpClient = _httpClientFactory.CreateClient("CompanyApi");
                var response = await httpClient.PutAsJsonAsync($"/api/companies/{id}/branding", vm);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Marka ayarları başarıyla güncellendi";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", $"Güncelleme başarısız: {errorContent}");
                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CompanyController.Branding] Exception occurred");
                ModelState.AddModelError("", $"Hata oluştu: {ex.Message}");
                return View(vm);
            }
        }
    }
}
