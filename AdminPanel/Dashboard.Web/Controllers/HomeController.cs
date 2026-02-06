using System;
using System.Collections.Generic;
using System.Linq;
using Dashboard.Web.Models;
using Dashboard.Web.Services;
using ECommerce.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Dashboard.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly Dashboard.Web.Services.DashboardApiService _dashboardApiService;
    private readonly Dashboard.Web.Services.IApiService<ECommerce.Application.DTOs.ProductDto> _productService;
    private readonly Dashboard.Web.Services.IApiService<ECommerce.Application.DTOs.OrderDto> _orderService;
    private readonly Dashboard.Web.Services.IApiService<ECommerce.Application.DTOs.CustomerDto> _customerService;
    private readonly Dashboard.Web.Services.IApiService<ECommerce.Application.DTOs.CompanyDto> _companyService;
    private readonly Dashboard.Web.Services.IApiService<CategoryDto> _categoryService;
    private readonly Dashboard.Web.Services.IApiService<ECommerce.Application.DTOs.BrandDto> _brandService;
    private readonly Dashboard.Web.Services.NotificationApiService _notificationService;
    private readonly Dashboard.Web.Services.IApiService<ECommerce.Application.DTOs.CampaignDto> _campaignService;
    private readonly Dashboard.Web.Services.UserManagementApiService _userManagementService;
    private readonly Dashboard.Web.Services.CustomerMessageApiService _messageService;

    public HomeController(
        Microsoft.Extensions.Logging.ILogger<HomeController> logger,
        Dashboard.Web.Services.DashboardApiService dashboardApiService,
        Dashboard.Web.Services.IApiService<ECommerce.Application.DTOs.ProductDto> productService,
        Dashboard.Web.Services.IApiService<ECommerce.Application.DTOs.OrderDto> orderService,
        Dashboard.Web.Services.IApiService<ECommerce.Application.DTOs.CustomerDto> customerService,
        Dashboard.Web.Services.IApiService<ECommerce.Application.DTOs.CompanyDto> companyService,
        Dashboard.Web.Services.IApiService<CategoryDto> categoryService,
        Dashboard.Web.Services.IApiService<ECommerce.Application.DTOs.BrandDto> brandService,
        Dashboard.Web.Services.NotificationApiService notificationService,
        Dashboard.Web.Services.IApiService<ECommerce.Application.DTOs.CampaignDto> campaignService,
        Dashboard.Web.Services.UserManagementApiService userManagementService,
        Dashboard.Web.Services.CustomerMessageApiService messageService)
    {
        _logger = logger;
        _dashboardApiService = dashboardApiService;
        _productService = productService;
        _orderService = orderService;
        _customerService = customerService;
        _companyService = companyService;
        _categoryService = categoryService;
        _brandService = brandService;
        _notificationService = notificationService;
        _campaignService = campaignService;
        _userManagementService = userManagementService;
        _messageService = messageService;
    }

    [ResponseCache(Duration = 120, VaryByQueryKeys = new[] { "companyId" })]
    public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, int? companyId)
    {
        var isSuperAdmin = User.IsInRole("SuperAdmin");
        if (isSuperAdmin)
        {
            try
            {
                var companies = await _companyService.GetAllAsync();
                ViewBag.Companies = companies?.Data?.Select(c => new Dashboard.Web.Models.CompanySelectVm
                {
                    Id = c.Id,
                    Name = c.Name
                }).ToList() ?? new List<Dashboard.Web.Models.CompanySelectVm>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Şirket listesi alınamadı: {Message}", ex.Message);
            }
        }
        else
        {
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out var parsedCompanyId))
            {
                _logger.LogWarning("CompanyId claim bulunamadı");
                return Forbid();
            }
            companyId = parsedCompanyId;
        }

        // API'den DashboardKpiViewModel verisini çek
        var kpiData = await _dashboardApiService.GetKpiAsync(companyId: companyId, startDate: startDate, endDate: endDate);

        if (kpiData == null)
        {
            _logger.LogWarning("Dashboard istatistikleri alınamadı, boş model gönderiliyor");
            return View(new Dashboard.Web.Models.DashboardStatsVm());
        }

        // DashboardStatsVm'ye eşle (View bu modeli bekliyor)
        var statsVm = new Dashboard.Web.Models.DashboardStatsVm
        {
            TotalProducts = kpiData.Products.TotalProducts,
            TotalOrders = kpiData.Orders.TotalOrders,
            TotalCustomers = kpiData.Customers.TotalCustomers,
            TotalSales = (decimal)kpiData.Sales.MonthlySales,
            TopProducts = kpiData.TopProducts,
            LowStockProducts = kpiData.LowStockProducts,
            Sales = kpiData.Sales,
            Orders = kpiData.Orders,
            Customers = kpiData.Customers,
            CustomerSegmentation = kpiData.CustomerSegmentation,
            RevenueTrendJson = kpiData.RevenueTrendJson,
            OrderStatusJson = kpiData.OrderStatusJson,
            CustomerSegmentJson = kpiData.CustomerSegmentJson
        };

        return View(statsVm);
    }

    #region AJAX Endpoints for Charts (Removed - Not Used)

    /// <summary>
    /// Satış trendi verilerini getirir (Line Chart)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetSalesTrend(DateTime? startDate, DateTime? endDate, int? companyId)
    {
        companyId = ResolveCompanyId(companyId);
        var trendData = await _dashboardApiService.GetRevenueTrendAsync(companyId);

        var data = trendData?.Select(r => new
        {
            date = r.Date.ToString("dd MMM"),
            fullDate = r.Date.ToString("yyyy-MM-dd"),
            revenue = r.Revenue,
            orders = r.OrderCount
        }) ?? Enumerable.Empty<object>();
        return Json(data);
    }

    /// <summary>
    /// Kategori bazlı satış dağılımını getirir (Pie Chart)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCategorySales(DateTime? startDate, DateTime? endDate, int? companyId)
    {
        companyId = ResolveCompanyId(companyId);
        var categorySales = await _dashboardApiService.GetCategorySalesAsync(startDate, endDate, companyId);

        return Json(categorySales?.Select(c => new
        {
            id = c.CategoryId,
            name = c.CategoryName,
            value = c.TotalSales,
            percentage = c.Percentage,
            color = c.Color
        }) ?? Enumerable.Empty<object>());
    }

    /// <summary>
    /// Kategori bazlı stok dağılımını getirir (Pie Chart)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCategoryStock(DateTime? startDate, DateTime? endDate, int? companyId)
    {
        companyId = ResolveCompanyId(companyId);
        var categoryStock = await _dashboardApiService.GetCategoryStockAsync(startDate, endDate, companyId);

        return Json(categoryStock?.Select(c => new
        {
            id = c.CategoryId,
            name = c.CategoryName,
            value = c.StockQuantity,
            percentage = c.Percentage,
            color = c.Color
        }) ?? Enumerable.Empty<object>());
    }

    /// <summary>
    /// Müşteri segmentasyonu verilerini getirir (Bar Chart)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCustomerSegmentation(DateTime? startDate, DateTime? endDate, int? companyId)
    {
        companyId = ResolveCompanyId(companyId);
        var kpiData = await _dashboardApiService.GetKpiAsync(startDate, endDate, companyId);

        var segment = kpiData?.CustomerSegmentation;
        return Json(new
        {
            newCustomers = segment?.NewCustomers ?? 0,
            returningCustomers = segment?.ReturningCustomers ?? 0,
            newRevenue = segment?.NewCustomersRevenue ?? 0,
            returningRevenue = segment?.ReturningCustomersRevenue ?? 0,
            newPercent = segment?.NewCustomerPercent ?? 0,
            returningPercent = segment?.ReturningCustomerPercent ?? 0
        });
    }

    /// <summary>
    /// Sipariş durumu dağılımını getirir (Stacked Bar Chart)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetOrderStatusDistribution(DateTime? startDate, DateTime? endDate, int? companyId)
    {
        companyId = ResolveCompanyId(companyId);
        var distribution = await _dashboardApiService.GetOrderStatusDistributionAsync(startDate, endDate, companyId);

        return Json(distribution?.Select(d => new
        {
            date = d.Date.ToString("dd MMM"),
            pending = d.PendingCount,
            shipped = d.ShippedCount,
            delivered = d.DeliveredCount,
            cancelled = d.CancelledCount
        }) ?? Enumerable.Empty<object>());
    }

    /// <summary>
    /// Coğrafi dağılım verilerini getirir (Heatmap)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetGeographicDistribution(DateTime? startDate, DateTime? endDate, int? companyId)
    {
        companyId = ResolveCompanyId(companyId);
        var geographicData = await _dashboardApiService.GetGeographicDistributionAsync(startDate, endDate, companyId);

        return Json(geographicData?.Select(g => new
        {
            country = g.Country,
            city = g.City,
            latitude = g.Latitude,
            longitude = g.Longitude,
            value = g.OrderCount
        }) ?? Enumerable.Empty<object>());
    }

    /// <summary>
    /// Ortalama sepet tutarı trendini getirir (Line Chart)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAverageCartTrend(DateTime? startDate, DateTime? endDate, int? companyId)
    {
        companyId = ResolveCompanyId(companyId);
        var averageCartTrend = await _dashboardApiService.GetAverageCartTrendAsync(startDate, endDate, companyId);

        return Json(averageCartTrend?.Select(t => new
        {
            date = t.Date.ToString("dd MMM"),
            fullDate = t.Date.ToString("yyyy-MM-dd"),
            averageCartValue = t.AverageCartValue
        }) ?? Enumerable.Empty<object>());
    }

    /// <summary>
    /// En çok satan ürünleri getirir (Horizontal Bar Chart)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetTopProducts(DateTime? startDate, DateTime? endDate, int? companyId)
    {
        companyId = ResolveCompanyId(companyId);
        var topProducts = await _dashboardApiService.GetTopProductsAsync(startDate, endDate, companyId);

        return Json(topProducts?.Select(p => new
        {
            id = p.ProductId,
            name = p.ProductName,
            quantity = p.QuantitySold,
            revenue = p.Revenue,
            category = p.CategoryName
        }) ?? Enumerable.Empty<object>());
    }

    /// <summary>
    /// Company ID'yi çözer - SuperAdmin değilse kullanıcının şirketini kullanır
    /// </summary>
    private int? ResolveCompanyId(int? requestedCompanyId)
    {
        var isSuperAdmin = User.IsInRole("SuperAdmin");

        if (!isSuperAdmin)
        {
            var userCompanyId = User.FindFirst("CompanyId")?.Value;
            if (!string.IsNullOrEmpty(userCompanyId) && int.TryParse(userCompanyId, out var parsedId))
            {
                return parsedId;
            }
        }

        return requestedCompanyId;
    }

    #endregion

    #region Notification Actions

    /// <summary>
    /// Bildirimler sayfası
    /// </summary>
    public async Task<IActionResult> Notifications()
    {
        var summary = await _notificationService.GetSummaryAsync();
        var allNotifications = await _notificationService.GetAllAsync();
        var lowStockProducts = await _notificationService.GetLowStockProductsAsync();
        var recentOrders = await _notificationService.GetRecentOrdersAsync();

        var viewModel = new NotificationsViewModel
        {
            Summary = summary ?? new NotificationSummaryVm(),
            AllNotifications = allNotifications ?? new List<NotificationDto>(),
            LowStockProducts = lowStockProducts ?? new List<LowStockItemVm>(),
            RecentOrders = recentOrders ?? new List<RecentOrderVm>()
        };

        return View(viewModel);
    }

    /// <summary>
    /// Bildirim özeti getirir (AJAX için)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetNotificationSummary()
    {
        var summary = await _notificationService.GetSummaryAsync();
        return Json(summary ?? new NotificationSummaryVm());
    }

    /// <summary>
    /// Düşük stoklu ürünleri getirir
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetLowStockProducts(int threshold = 10)
    {
        var products = await _notificationService.GetLowStockProductsAsync(threshold);
        return Json(products ?? new List<LowStockItemVm>());
    }

    /// <summary>
    /// Son siparişleri getirir
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetRecentOrders(int count = 10)
    {
        var orders = await _notificationService.GetRecentOrdersAsync(count);
        return Json(orders ?? new List<RecentOrderVm>());
    }

    /// <summary>
    /// Bildirimi okundu olarak işaretler
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> MarkNotificationAsRead(int id)
    {
        var result = await _notificationService.MarkAsReadAsync(id);
        return Json(new { success = result });
    }

    /// <summary>
    /// Tüm bildirimleri okundu olarak işaretler
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> MarkAllNotificationsAsRead()
    {
        var result = await _notificationService.MarkAllAsReadAsync();
        return Json(new { success = result });
    }

    /// <summary>
    /// Bildirimi siler
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> DeleteNotification(int id)
    {
        var result = await _notificationService.DeleteAsync(id);
        return Json(new { success = result });
    }

    /// <summary>
    /// Düşük stok kontrolü yapar
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CheckLowStock(int threshold = 10)
    {
        var result = await _notificationService.CheckLowStockAsync(threshold);
        return Json(new { success = result });
    }

    #endregion

    #region Quick Actions - Hızlı Aksiyon Butonları

    /// <summary>
    /// Hızlı ürün ekleme için kategorileri ve markaları getirir
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetQuickProductFormData()
    {
        try
        {
            var categories = await _categoryService.GetAllAsync();
            var brands = await _brandService.GetAllAsync();

            return Json(new
            {
                success = true,
                categories = categories?.Data?.Select(c => new { id = c.Id, name = c.Name }) ?? Enumerable.Empty<object>(),
                brands = brands?.Data?.Select(b => new { id = b.Id, name = b.Name }) ?? Enumerable.Empty<object>()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Quick product form data alınamadı");
            return Json(new { success = false, message = "Veri alınamadı" });
        }
    }

    /// <summary>
    /// Hızlı ürün ekleme
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> QuickAddProduct([FromBody] QuickProductAddVm model)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            var dto = new ECommerce.Application.DTOs.ProductCreateDto
            {
                Name = model.Name,
                Description = model.Description ?? "",
                Price = model.Price,
                StockQuantity = model.StockQuantity,
                CategoryId = model.CategoryId,
                BrandId = model.BrandId,
                CompanyId = companyId,
                ImageUrl = model.ImageUrl
            };

            var productDto = new ECommerce.Application.DTOs.ProductDto
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                CategoryId = dto.CategoryId,
                BrandId = dto.BrandId,
                CompanyId = dto.CompanyId,
                ImageUrl = dto.ImageUrl,
                IsActive = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            var response = await _productService.CreateAsync(productDto);
            return Json(new { success = response.Success, message = response.Success ? "Ürün başarıyla eklendi" : $"Ürün eklenemedi: {response.Message}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Hızlı ürün ekleme hatası");
            return Json(new { success = false, message = "Bir hata oluştu" });
        }
    }

    /// <summary>
    /// Aktif kampanyaları getirir
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetActiveCampaigns()
    {
        try
        {
            var response = await _campaignService.GetAllAsync();
            if (response?.Data != null)
            {
                var activeCampaigns = response.Data.Where(c => c.IsActive && c.IsCurrentlyActive).ToList();
                _logger.LogInformation("Active campaigns count: {Count}", activeCampaigns.Count);
                return Json(activeCampaigns);
            }
            return Json(new List<ECommerce.Application.DTOs.CampaignDto>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching active campaigns");
            return Json(new List<ECommerce.Application.DTOs.CampaignDto>());
        }
    }

    /// <summary>
    /// Kampanya özeti getirir
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCampaignSummary()
    {
        var allCampaigns = await _campaignService.GetAllAsync();
        var summary = new
        {
            Total = allCampaigns?.Data?.Count ?? 0,
            Active = allCampaigns?.Data != null ? allCampaigns.Data.Count(c => c.IsActive) : 0
        };
        return Json(summary);
    }

    /// <summary>
    /// Yeni kampanya oluşturur
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateCampaign([FromBody] Dashboard.Web.Models.CampaignCreateVm model)
    {
        try
        {
            model.CompanyId = GetCurrentCompanyId();
            var campaignDto = new ECommerce.Application.DTOs.CampaignDto
            {
                Name = model.Name,
                Description = model.Description,
                DiscountPercent = model.DiscountPercent,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                CompanyId = model.CompanyId,
                CreatedAt = DateTime.Now
            };
            var response = await _campaignService.CreateAsync(campaignDto);
            return Json(new { success = response.Success, message = response.Success ? "Kampanya başarıyla oluşturuldu" : $"Kampanya oluşturulamadı: {response.Message}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kampanya oluşturma hatası");
            return Json(new { success = false, message = "Bir hata oluştu" });
        }
    }

    /// <summary>
    /// Kampanyayı sonlandırır
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> DeactivateCampaign(int id)
    {
        var result = await _campaignService.PostActionAsync($"{id}/deactivate", new { });
        return Json(new { success = result });
    }

    /// <summary>
    /// Sipariş arar (Quick Order Update için)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> SearchOrder(int orderId)
    {
        try
        {
            var ordersResponse = await _orderService.GetAllAsync();
            var order = ordersResponse?.Data?.FirstOrDefault(o => o.Id == orderId);

            if (order == null)
                return Json(new { success = false, message = "Sipariş bulunamadı" });

            return Json(new
            {
                success = true,
                order = new
                {
                    id = order.Id,
                    customerName = order.CustomerName,
                    status = order.Status.ToString(),
                    statusText = order.StatusText,
                    totalAmount = order.TotalAmount,
                    orderDate = order.OrderDate.ToString("dd MMM yyyy HH:mm"),
                    itemCount = order.Items?.Count ?? 0
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sipariş arama hatası");
            return Json(new { success = false, message = "Bir hata oluştu" });
        }
    }

    /// <summary>
    /// Sipariş durumunu günceller
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> UpdateOrderStatus([FromBody] QuickOrderUpdateVm model)
    {
        try
        {
            if (!Enum.TryParse<OrderStatus>(model.Status, out var status))
                return Json(new { success = false, message = "Geçersiz durum" });

            var result = await _orderService.PostActionAsync($"{model.OrderId}/status", new { Status = status });
            return Json(new { success = result, message = result ? "Sipariş durumu güncellendi" : "Güncelleme başarısız" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sipariş güncelleme hatası");
            return Json(new { success = false, message = "Bir hata oluştu" });
        }
    }

    /// <summary>
    /// Müşteri mesajları özeti getirir
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMessageSummary()
    {
        var summary = await _messageService.GetSummaryAsync();
        return Json(summary ?? new CustomerMessageSummaryVm());
    }

    /// <summary>
    /// Bekleyen müşteri mesajlarını getirir
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPendingMessages()
    {
        var messages = await _messageService.GetPendingAsync();
        return Json(messages ?? new List<CustomerMessageVm>());
    }

    /// <summary>
    /// Mesajı okundu olarak işaretler
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> MarkMessageAsRead(int id)
    {
        var result = await _messageService.MarkAsReadAsync(id);
        return Json(new { success = result });
    }

    /// <summary>
    /// Müşteri mesajına yanıt gönderir
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ReplyToMessage([FromBody] QuickMessageReplyVm model)
    {
        try
        {
            var userId = GetCurrentUserId();
            var reply = new MessageReplyVm
            {
                MessageId = model.MessageId,
                Reply = model.Reply,
                RepliedByUserId = userId
            };

            var result = await _messageService.ReplyAsync(model.MessageId, reply);
            return Json(new { success = result, message = result ? "Yanıt gönderildi" : "Yanıt gönderilemedi" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Mesaj yanıtlama hatası");
            return Json(new { success = false, message = "Bir hata oluştu" });
        }
    }

    #endregion

    #region ==================== LOGIN HISTORY / USER MANAGEMENT ENDPOINTS ====================

    /// <summary>
    /// Son giriş yapan kullanıcılar (Dashboard widget)
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "CanManageUsers")]
    /// <summary>
    /// Kullanıcı yönetimi özeti (Dashboard widget)
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "CanManageUsers")]
    public async Task<IActionResult> GetUserManagementSummary()
    {
        try
        {
            var summary = await _userManagementService.GetSummaryAsync();
            return Json(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kullanıcı özeti getirme hatası");
            return Json(new UserManagementSummaryVm());
        }
    }

    #endregion

    /// <summary>
    /// Geçerli kullanıcının şirket ID'sini döndürür
    /// </summary>
    private int GetCurrentCompanyId()
    {
        var companyIdClaim = User.FindFirst("CompanyId")?.Value;
        if (!string.IsNullOrEmpty(companyIdClaim) && int.TryParse(companyIdClaim, out var companyId))
            return companyId;
        return 1; // Default company
    }

    /// <summary>
    /// Geçerli kullanıcının ID'sini döndürür
    /// </summary>
    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("UserId")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var userId))
            return userId;
        return 1; // Default user
    }
}
