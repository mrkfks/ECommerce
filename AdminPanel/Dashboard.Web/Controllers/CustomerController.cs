using Dashboard.Web.Services;
using ECommerce.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dashboard.Web.Controllers
{
    [Authorize(Roles = "CompanyAdmin,SuperAdmin,User")]
    public class CustomerController : Controller
    {
        private readonly IApiService<CustomerDto> _customerService;

        public CustomerController(IApiService<CustomerDto> customerService)
        {
            _customerService = customerService;
        }

        // Listeleme
        public async Task<IActionResult> Index()
        {
            try
            {
                // Varsayılan olarak ilk sayfa ve 100 kayıt çekiliyor, ihtiyaca göre değiştirilebilir
                var pagedResult = await _customerService.GetPagedListAsync(1, 100);
                return View(pagedResult.Items?.ToList() ?? new List<CustomerDto>());
            }
            catch (Exception ex)
            {
                // Hata loglama
                ViewBag.ErrorMessage = ex.Message;
                return View(new List<CustomerDto>());
            }
        }

        // Detay
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null || customer.Data == null)
                return NotFound();

            return View(customer.Data);
        }

        // Düzenleme
        [HttpGet]
        [Authorize(Roles = "CompanyAdmin,SuperAdmin,User")]
        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null || customer.Data == null)
                return NotFound();

            return View(customer.Data);
        }

        [HttpPost]
        [Authorize(Roles = "CompanyAdmin,SuperAdmin,User")]
        public async Task<IActionResult> Edit(CustomerDto customer)
        {
            if (!ModelState.IsValid)
                return View(customer);

            var response = await _customerService.UpdateAsync(customer.Id, customer);
            if (response != null && response.Success)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Müşteri güncellenirken hata oluştu.");
            return View(customer);
        }

        // Silme
        [HttpGet]
        [Authorize(Roles = "CompanyAdmin,SuperAdmin,User")]
        public async Task<IActionResult> Delete(int id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null || customer.Data == null)
                return NotFound();

            return View(customer.Data);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "CompanyAdmin,SuperAdmin,User")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await _customerService.DeleteAsync(id);
            if (response != null && response.Success)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Müşteri silinirken hata oluştu.");
            var customer = await _customerService.GetByIdAsync(id);
            return View(customer);
        }
    }
}
