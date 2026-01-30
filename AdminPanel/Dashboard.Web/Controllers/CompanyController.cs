using Dashboard.Web.Models;
using Dashboard.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CompanyDto = ECommerce.Application.DTOs.CompanyDto;

namespace Dashboard.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class CompanyController : Controller
    {
        private readonly IApiService<ECommerce.Application.DTOs.CompanyDto> _companyService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public CompanyController(
            IApiService<ECommerce.Application.DTOs.CompanyDto> companyService,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _companyService = companyService;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            var response = await _companyService.GetAllAsync();
            var companies = response?.Data ?? new List<CompanyDto>();
            var companyVms = companies.Select(c => new CompanyVm
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
                UpdatedAt = c.UpdatedAt,
                // Branding fields
                Domain = c.Domain,
                LogoUrl = c.LogoUrl,
                PrimaryColor = c.PrimaryColor,
                SecondaryColor = c.SecondaryColor
            }).ToList();
            return View(companyVms);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CompanyDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var response = await _companyService.CreateAsync(dto);

            if (response != null && response.Success)
            {
                TempData["Success"] = "Şirket başarıyla kaydedildi";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Şirket kaydedilirken hata oluştu");
            return View(dto);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var response = await _companyService.GetByIdAsync(id);
            var company = response?.Data;
            if (company == null)
                return NotFound();

            var companyVm = new CompanyVm
            {
                Id = company.Id,
                Name = company.Name,
                Address = company.Address,
                PhoneNumber = company.PhoneNumber,
                Email = company.Email,
                TaxNumber = company.TaxNumber,
                ResponsiblePersonName = company.ResponsiblePersonName,
                ResponsiblePersonPhone = company.ResponsiblePersonPhone,
                ResponsiblePersonEmail = company.ResponsiblePersonEmail,
                IsActive = company.IsActive,
                IsApproved = company.IsApproved,
                CreatedAt = company.CreatedAt,
                UpdatedAt = company.UpdatedAt,
                // Branding fields
                Domain = company.Domain,
                LogoUrl = company.LogoUrl,
                PrimaryColor = company.PrimaryColor,
                SecondaryColor = company.SecondaryColor
            };
            return View(companyVm);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var response = await _companyService.GetByIdAsync(id);
            var company = response?.Data;
            if (company == null)
                return NotFound();

            return View(company);
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

        [HttpGet]
        public async Task<IActionResult> Branding(int id)
        {
            var response = await _companyService.GetByIdAsync(id);
            var company = response?.Data;
            if (company == null)
                return NotFound();

            var vm = new CompanyBrandingVm
            {
                Id = company.Id,
                Name = company.Name,
                Domain = company.Domain,
                LogoUrl = company.LogoUrl,
                PrimaryColor = company.PrimaryColor ?? "#3b82f6",
                SecondaryColor = company.SecondaryColor ?? "#1e40af"
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Branding(int id, CompanyBrandingVm model, IFormFile? logoFile)
        {
            if (id != model.Id)
                return BadRequest();

            try
            {
                var client = _httpClientFactory.CreateClient("ApiClient");

                // Upload logo if provided
                if (logoFile != null && logoFile.Length > 0)
                {
                    using var content = new MultipartFormDataContent();
                    using var stream = logoFile.OpenReadStream();
                    var fileContent = new StreamContent(stream);
                    content.Add(fileContent, "file", logoFile.FileName);

                    var uploadResponse = await client.PostAsync($"api/files/company/{id}", content);
                    if (uploadResponse.IsSuccessStatusCode)
                    {
                        var result = await uploadResponse.Content.ReadFromJsonAsync<ApiResult<string>>();
                        if (result?.Data != null)
                        {
                            model.LogoUrl = result.Data;
                        }
                    }
                }

                // Update branding
                var brandingDto = new
                {
                    Domain = model.Domain,
                    LogoUrl = model.LogoUrl,
                    PrimaryColor = model.PrimaryColor,
                    SecondaryColor = model.SecondaryColor
                };

                var brandingResponse = await client.PutAsJsonAsync($"api/companies/{id}/branding", brandingDto);

                if (brandingResponse.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Marka ayarları başarıyla güncellendi";
                    return RedirectToAction(nameof(Details), new { id });
                }

                TempData["Error"] = "Marka ayarları güncellenirken hata oluştu";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Hata: {ex.Message}";
            }

            return View(model);
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
    }

    public class ApiResult<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
    }
}

