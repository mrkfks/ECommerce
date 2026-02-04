using Dashboard.Web.Services;
using ECommerce.Application.DTOs;
using ECommerce.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

            // Dropdown için status listesi
            ViewBag.StatusList = Enum.GetValues<OrderStatus>()
                .Select(s => new { Value = (int)s, Text = GetStatusText(s) })
                .ToList();

            return View(response.Data);
        }

        [Authorize(Roles = "CompanyAdmin,SuperAdmin,User")]
        [HttpPost]
        public async Task<IActionResult> Edit(int id, OrderStatus status)
        {
            var success = await _orderService.UpdateStatusAsync(id, status);
            if (success)
            {
                TempData["Success"] = "Sipariş durumu başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Sipariş güncellenirken hata oluştu.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        private string GetStatusText(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "Beklemede",
                OrderStatus.Processing => "İşleniyor",
                OrderStatus.Shipped => "Kargoda",
                OrderStatus.Delivered => "Teslim Edildi",
                OrderStatus.Cancelled => "İptal Edildi",
                OrderStatus.Received => "Teslim Alındı",
                OrderStatus.Completed => "Tamamlandı",
                _ => status.ToString()
            };
        }
    }

}