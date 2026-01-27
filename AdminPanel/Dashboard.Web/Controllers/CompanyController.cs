using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dashboard.Web.Services;
using Dashboard.Web.Models;
using ECommerce.Application.DTOs;

namespace Dashboard.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class CompanyController : Controller
    {
        private readonly IApiService<CompanyDto> _companyService;

        public CompanyController(IApiService<CompanyDto> companyService)
        {
            _companyService = companyService;
        }

        public async Task<IActionResult> Index()
        {
            var companies = await _companyService.GetAllAsync();
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
                UpdatedAt = c.UpdatedAt
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
            
            var success = await _companyService.CreateAsync(dto);
            
            if (success)
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
            var company = await _companyService.GetByIdAsync(id);
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
                UpdatedAt = company.UpdatedAt
            };
            return View(companyVm);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var company = await _companyService.GetByIdAsync(id);
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
}
