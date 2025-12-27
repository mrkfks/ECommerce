using ECommerce.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dashboard.Web.Models;
using Dashboard.Web.Services;

namespace Dashboard.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ProductApiService _productService;
    private readonly OrderApiService _orderService;
    private readonly CustomerApiService _customerService;

    public HomeController(
        ILogger<HomeController> logger,
        ProductApiService productService,
        OrderApiService orderService,
        CustomerApiService customerService)
    {
        _logger = logger;
        _productService = productService;
        _orderService = orderService;
        _customerService = customerService;
    }

    public async Task<IActionResult> Index()
    {
        var isSuperAdmin = User.IsInRole("SuperAdmin");
        var companyId = 0;

        if (!isSuperAdmin)
        {
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
            {
                _logger.LogWarning("CompanyId claim bulunamadı");
                return Forbid();
            }
        }

        var productsTask = _productService.GetAllAsync();
        var ordersTask = _orderService.GetAllAsync();
        var customersTask = _customerService.GetAllAsync();

        await Task.WhenAll(productsTask, ordersTask, customersTask);

        var products = await productsTask;
        var orders = await ordersTask;
        var customers = await customersTask;

        // Şirket filtreleme
        if (!isSuperAdmin)
        {
            products = products.Where(p => p.CompanyId == companyId).ToList();
            orders = orders.Where(o => o.CompanyId == companyId).ToList();
            customers = customers.Where(c => c.CompanyId == companyId).ToList();
        }

        var viewModel = new DashboardStatsVm
        {
            TotalProducts = products.Count,
            TotalOrders = orders.Count,
            TotalCustomers = customers.Count,
            TotalSales = orders.Sum(o => o.TotalAmount)
        };

        return View(viewModel);
    }
}
