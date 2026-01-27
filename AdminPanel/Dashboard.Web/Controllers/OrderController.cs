using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.DTOs;
using Dashboard.Web.Services;

namespace Dashboard.Web.Controllers
{
    [Authorize(Roles = "CompanyAdmin,SuperAdmin,User")]
    public class OrderController : Controller
    {
        private readonly IApiService<OrderDto> _orderService;

        public OrderController(IApiService<OrderDto> orderService)
        {
            _orderService = orderService;
        }

        // Listeleme
        public async Task<IActionResult> Index()
        {
            var response = await _orderService.GetAllAsync();
            if (response == null || response.Data == null)
                return View(new List<OrderDto>());
            return View(response.Data);
        }

        // Detay
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var response = await _orderService.GetByIdAsync(id);
            if (response == null || response.Data == null)
                return NotFound();
            return View(response.Data);
        }

        // Durum güncelleme (Edit)
        [Authorize(Roles = "CompanyAdmin,SuperAdmin,User")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var response = await _orderService.GetByIdAsync(id);
            if (response == null || response.Data == null)
                return NotFound();
            return View(response.Data);
        }

        [Authorize(Roles = "CompanyAdmin,SuperAdmin,User")]
        [HttpPost]
        public async Task<IActionResult> Edit(OrderDto order)
        {
            if (!ModelState.IsValid)
                return View(order);

            var response = await _orderService.UpdateAsync(order.Id, order);
            if (response != null && response.Success)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Sipariş güncellenirken hata oluştu.");
            return View(order);
        }
    }

}