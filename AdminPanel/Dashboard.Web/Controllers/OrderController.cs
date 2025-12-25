using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.DTOs;
using Dashboard.Web.Services;

namespace Dashboard.Web.Controllers
{
    [Authorize(Roles = "CompanyAdmin,SuperAdmin,User")]
    public class OrderController : Controller
    {
        private readonly OrderApiService _orderService;

        public OrderController(OrderApiService orderService)
        {
            _orderService = orderService;
        }

        // Listeleme
        public async Task<IActionResult> Index()
        {
            var orders = await _orderService.GetAllAsync();
            return View(orders);
        }

        // Detay
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order == null)
                return NotFound();

            return View(order);
        }

        // Durum güncelleme (Edit)
        [Authorize(Roles = "CompanyAdmin,SuperAdmin,User")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order == null)
                return NotFound();

            return View(order);
        }

        [Authorize(Roles = "CompanyAdmin,SuperAdmin,User")]
        [HttpPost]
        public async Task<IActionResult> Edit(OrderDto order)
        {
            if (!ModelState.IsValid)
                return View(order);

            var success = await _orderService.UpdateAsync(order.Id, order);
            if (success)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Sipariş güncellenirken hata oluştu.");
            return View(order);
        }
    }

}