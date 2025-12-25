using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ECommerce.Application.DTOs;
using Dashboard.Web.Services;

namespace Dashboard.Web.Controllers
{
    [Authorize(Roles = "CompanyAdmin,SuperAdmin,User")]
    public class CustomerController : Controller
    {
        private readonly CustomerApiService _customerService;

        public CustomerController(CustomerApiService customerService)
        {
            _customerService = customerService;
        }

        // Listeleme
        public async Task<IActionResult> Index()
        {
            var customers = await _customerService.GetAllAsync();
            return View(customers);
        }

        // Detay
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null)
                return NotFound();

            return View(customer);
        }

        // Düzenleme
        [HttpGet]
        [Authorize(Roles = "CompanyAdmin,SuperAdmin,User")]
        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null)
                return NotFound();

            return View(customer);
        }

        [HttpPost]
        [Authorize(Roles = "CompanyAdmin,SuperAdmin,User")]
        public async Task<IActionResult> Edit(CustomerDto customer)
        {
            if (!ModelState.IsValid)
                return View(customer);

            var success = await _customerService.UpdateAsync(customer.Id, customer);
            if (success)
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
            if (customer == null)
                return NotFound();

            return View(customer);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "CompanyAdmin,SuperAdmin,User")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var success = await _customerService.DeleteAsync(id);
            if (success)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Müşteri silinirken hata oluştu.");
            var customer = await _customerService.GetByIdAsync(id);
            return View(customer);
        }
    }
}
