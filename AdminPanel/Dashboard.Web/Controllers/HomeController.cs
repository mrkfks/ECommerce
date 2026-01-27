

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Dashboard.Web.Models;



namespace Dashboard.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly Dashboard.Web.Services.IApiService<Dashboard.Web.Models.DashboardStatsVm> _dashboardStatsService;
    private readonly Dashboard.Web.Services.IApiService<Dashboard.Web.Models.DashboardKpiViewModel> _dashboardService;
    private readonly Dashboard.Web.Services.IApiService<ProductDto> _productService;
    private readonly Dashboard.Web.Services.IApiService<OrderDto> _orderService;
    private readonly Dashboard.Web.Services.IApiService<CustomerDto> _customerService;
    private readonly Dashboard.Web.Services.IApiService<CompanyDto> _companyService;
    private readonly Dashboard.Web.Services.IApiService<CategoryDto> _categoryService;
    private readonly Dashboard.Web.Services.IApiService<BrandDto> _brandService;
    private readonly Dashboard.Web.Services.NotificationApiService _notificationService;
    private readonly Dashboard.Web.Services.IApiService<CampaignDto> _campaignService;
    private readonly Dashboard.Web.Services.LoginHistoryApiService _loginHistoryService;
    private readonly Dashboard.Web.Services.UserManagementApiService _userManagementService;
    private readonly Dashboard.Web.Services.CustomerMessageApiService _messageService;

    public HomeController(
        Microsoft.Extensions.Logging.ILogger<HomeController> logger,
        Dashboard.Web.Services.IApiService<Dashboard.Web.Models.DashboardStatsVm> dashboardStatsService,
        Dashboard.Web.Services.IApiService<Dashboard.Web.Models.DashboardKpiViewModel> dashboardService,
        Dashboard.Web.Services.IApiService<ProductDto> productService,
        Dashboard.Web.Services.IApiService<OrderDto> orderService,
        Dashboard.Web.Services.IApiService<CustomerDto> customerService,
        Dashboard.Web.Services.IApiService<CompanyDto> companyService,
        Dashboard.Web.Services.IApiService<CategoryDto> categoryService,
        Dashboard.Web.Services.IApiService<BrandDto> brandService,
        Dashboard.Web.Services.NotificationApiService notificationService,
        Dashboard.Web.Services.IApiService<CampaignDto> campaignService,
        Dashboard.Web.Services.LoginHistoryApiService loginHistoryService,
        Dashboard.Web.Services.UserManagementApiService userManagementService,
        Dashboard.Web.Services.CustomerMessageApiService messageService)
    {
        _logger = logger;
        _dashboardStatsService = dashboardStatsService;
        _dashboardService = dashboardService;
        _productService = productService;
        _orderService = orderService;
        _customerService = customerService;
        _companyService = companyService;
        _categoryService = categoryService;
        _brandService = brandService;
        _notificationService = notificationService;
        _campaignService = campaignService;
        _loginHistoryService = loginHistoryService;
        _userManagementService = userManagementService;
        _messageService = messageService;
    }

    [ResponseCache(Duration = 120, VaryByQueryKeys = new[] { "companyId" })]
    public async Task<IActionResult> Index()
    {
        var isSuperAdmin = User.IsInRole("SuperAdmin");
        int? companyId = null;

        if (!isSuperAdmin)
        {
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out var parsedCompanyId))
            {
                _logger.LogWarning("CompanyId claim bulunamadı");
                return Forbid();
            }
            companyId = parsedCompanyId;
        }

        // API'den DashboardStatsVm verisini çek
        var response = await _dashboardStatsService.GetByIdAsync(0); // 0 veya uygun parametre, endpoint'e göre
        if (response == null || response.Data == null)
        {
            _logger.LogWarning("Dashboard istatistikleri alınamadı, fallback kullanılıyor");
            return View(new Dashboard.Web.Models.DashboardKpiViewModel());
        }
        else
        {
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
            return View(response.Data);
        }
    }

            // ...existing code...
    #region AJAX Endpoints for Charts

    /// <summary>
    /// Satış trendi verilerini getirir (Line Chart)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetSalesTrend(DateTime? startDate, DateTime? endDate, int? companyId)
    {
        companyId = ResolveCompanyId(companyId);
        var kpiData = (await _dashboardService.GetAllAsync())?.Data?.FirstOrDefault();
        var data = kpiData?.RevenueTrend?.Select(r => new
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
        var kpiData = (await _dashboardService.GetAllAsync())?.Data?.FirstOrDefault();

        // DashboardKpiViewModel'de CategorySales yok, dummy boş koleksiyon döndürülüyor
        return Json(Enumerable.Empty<object>());
    }

    /// <summary>
    /// Kategori bazlı stok dağılımını getirir (Pie Chart)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCategoryStock(DateTime? startDate, DateTime? endDate, int? companyId)
    {
        companyId = ResolveCompanyId(companyId);
        var kpiData = (await _dashboardService.GetAllAsync())?.Data?.FirstOrDefault();

        // DashboardKpiViewModel'de CategoryStock yok, dummy boş koleksiyon döndürülüyor
        return Json(Enumerable.Empty<object>());
    }

    /// <summary>
    /// Müşteri segmentasyonu verilerini getirir (Bar Chart)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCustomerSegmentation(DateTime? startDate, DateTime? endDate, int? companyId)
    {
        companyId = ResolveCompanyId(companyId);
        var kpiData = (await _dashboardService.GetAllAsync())?.Data?.FirstOrDefault();

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
        var kpiData = (await _dashboardService.GetAllAsync())?.Data?.FirstOrDefault();

        // DashboardKpiViewModel'de OrderStatusDistribution yok, dummy boş koleksiyon döndürülüyor
        return Json(Enumerable.Empty<object>());
    }

    /// <summary>
    /// Coğrafi dağılım verilerini getirir (Heatmap)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetGeographicDistribution(DateTime? startDate, DateTime? endDate, int? companyId)
    {
        companyId = ResolveCompanyId(companyId);
        var kpiData = (await _dashboardService.GetAllAsync())?.Data?.FirstOrDefault();

        // DashboardKpiViewModel'de GeographicDistribution yok, dummy boş koleksiyon döndürülüyor
        return Json(Enumerable.Empty<object>());
    }

    /// <summary>
    /// Ortalama sepet tutarı trendini getirir (Line Chart)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAverageCartTrend(DateTime? startDate, DateTime? endDate, int? companyId)
    {
        companyId = ResolveCompanyId(companyId);
        var kpiData = (await _dashboardService.GetAllAsync())?.Data?.FirstOrDefault();

        // DashboardKpiViewModel'de AverageCartTrend yok, dummy boş koleksiyon döndürülüyor
        return Json(Enumerable.Empty<object>());
    }

    /// <summary>
    /// En çok satan ürünleri getirir (Horizontal Bar Chart)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetTopProducts(DateTime? startDate, DateTime? endDate, int? companyId)
    {
        companyId = ResolveCompanyId(companyId);
        var kpiData = (await _dashboardService.GetAllAsync())?.Data?.FirstOrDefault();

        return Json(kpiData?.TopProducts?.Select(p => new
        {
            id = p.Id,
            name = !string.IsNullOrEmpty(p.Name) ? p.Name : p.ProductName,
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
            AllNotifications = allNotifications ?? new List<NotificationVm>(),
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
                categories = categories?.Select(c => new { id = c.Id, name = c.Name }) ?? Enumerable.Empty<object>(),
                brands = brands?.Select(b => new { id = b.Id, name = b.Name }) ?? Enumerable.Empty<object>()
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
            var dto = new ProductCreateDto
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

            var result = await _productService.CreateAsync(dto);
            return Json(new { success = result, message = result ? "Ürün başarıyla eklendi" : "Ürün eklenemedi" });
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
        var campaigns = await _campaignService.GetAllAsync();
        return Json(campaigns?.Data != null ? campaigns.Data.Where(c => c.IsActive) : new List<CampaignDto>());
    }

    /// <summary>
    /// Kampanya özeti getirir
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCampaignSummary()
    {
        var allCampaigns = await _campaignService.GetAllAsync();
        var summary = new {
            Total = allCampaigns?.Data?.Count ?? 0,
            Active = allCampaigns?.Data != null ? allCampaigns.Data.Count(c => c.IsActive) : 0
        };
        return Json(summary);
    }

    /// <summary>
    /// Yeni kampanya oluşturur
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateCampaign([FromBody] CampaignCreateVm model)
    {
        try
        {
            model.CompanyId = GetCurrentCompanyId();
            var dto = MapToCampaignDto(model);
            var result = await _campaignService.CreateAsync(dto);
            return Json(new { success = result, message = result ? "Kampanya başarıyla oluşturuldu" : "Kampanya oluşturulamadı" });
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
    public async Task<IActionResult> GetRecentLogins(int take = 10)
    {
        try
        {
            var logins = await _loginHistoryService.GetRecentAsync(take);
            return Json(logins);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Son girişler getirme hatası");
            return Json(new List<LoginHistoryVm>());
        }
    }

    /// <summary>
    /// Giriş geçmişi özeti (Dashboard widget)
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "CanManageUsers")]
    public async Task<IActionResult> GetLoginHistorySummary()
    {
        try
        {
            var summary = await _loginHistoryService.GetSummaryAsync();
            return Json(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Giriş özeti getirme hatası");
            return Json(new LoginHistorySummaryVm());
        }
    }

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
