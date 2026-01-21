using ECommerce.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dashboard.Web.Models;
using Dashboard.Web.Services;
using ECommerce.Domain.Enums;

namespace Dashboard.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly DashboardApiService _dashboardService;
    private readonly ProductApiService _productService;
    private readonly OrderApiService _orderService;
    private readonly CustomerApiService _customerService;
    private readonly CompanyApiService _companyService;
    private readonly NotificationApiService _notificationService;
    private readonly CampaignApiService _campaignService;
    private readonly CustomerMessageApiService _messageService;
    private readonly IApiService<CategoryDto> _categoryService;
    private readonly IApiService<BrandDto> _brandService;
    private readonly LoginHistoryApiService _loginHistoryService;
    private readonly UserManagementApiService _userManagementService;

    public HomeController(
        ILogger<HomeController> logger,
        DashboardApiService dashboardService,
        ProductApiService productService,
        OrderApiService orderService,
        CustomerApiService customerService,
        CompanyApiService companyService,
        NotificationApiService notificationService,
        CampaignApiService campaignService,
        CustomerMessageApiService messageService,
        IApiService<CategoryDto> categoryService,
        IApiService<BrandDto> brandService,
        LoginHistoryApiService loginHistoryService,
        UserManagementApiService userManagementService)
    {
        _logger = logger;
        _dashboardService = dashboardService;
        _productService = productService;
        _orderService = orderService;
        _customerService = customerService;
        _companyService = companyService;
        _notificationService = notificationService;
        _campaignService = campaignService;
        _messageService = messageService;
        _categoryService = categoryService;
        _brandService = brandService;
        _loginHistoryService = loginHistoryService;
        _userManagementService = userManagementService;
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

        // KPI verilerini API'den çek
        var kpiData = await _dashboardService.GetKpiAsync(companyId: companyId);

        if (kpiData == null)
        {
            _logger.LogWarning("KPI verileri alınamadı, fallback kullanılıyor");

            // Fallback: Eski yöntem
            var productsTask = _productService.GetAllAsync();
            var ordersTask = _orderService.GetAllAsync();
            var customersTask = _customerService.GetAllAsync();

            await Task.WhenAll(productsTask, ordersTask, customersTask);

            var products = await productsTask;
            var orders = await ordersTask;
            var customers = await customersTask;

            if (!isSuperAdmin && companyId.HasValue)
            {
                products = products.Where(p => p.CompanyId == companyId).ToList();
                orders = orders.Where(o => o.CompanyId == companyId).ToList();
                customers = customers.Where(c => c.CompanyId == companyId).ToList();
            }

            var fallbackVm = new DashboardKpiViewModel
            {
                Sales = new SalesKpiVm
                {
                    DailySales = orders.Where(o => o.OrderDate.Date == DateTime.Today).Sum(o => o.TotalAmount),
                    MonthlySales = orders.Sum(o => o.TotalAmount)
                },
                Orders = new OrderKpiVm
                {
                    TotalOrders = orders.Count
                },
                Customers = new CustomerKpiVm
                {
                    TotalCustomers = customers.Count
                },
                TopProducts = new List<TopProductVm>(),
                LowStockProducts = new List<LowStockProductVm>(),
                RevenueTrend = new List<RevenueTrendVm>(),
                CustomerSegmentation = new CustomerSegmentationVm()
            };

            return View(fallbackVm);
        }

        var viewModel = new DashboardKpiViewModel
        {
            Sales = kpiData.Sales,
            Orders = kpiData.Orders,
            Customers = kpiData.Customers,
            TopProducts = kpiData.TopProducts,
            LowStockProducts = kpiData.LowStockProducts,
            RevenueTrend = kpiData.RevenueTrend,
            CustomerSegmentation = kpiData.CustomerSegmentation
        };

        return View(viewModel);
    }

    /// <summary>
    /// AJAX ile KPI verilerini getirir
    /// </summary>
    [HttpGet]
    [ResponseCache(Duration = 120, VaryByQueryKeys = new[] { "startDate", "endDate", "companyId" })]
    public async Task<IActionResult> GetKpiData(DateTime? startDate, DateTime? endDate, int? companyId)
    {
        var isSuperAdmin = User.IsInRole("SuperAdmin");

        if (!isSuperAdmin)
        {
            var userCompanyId = User.FindFirst("CompanyId")?.Value;
            if (!string.IsNullOrEmpty(userCompanyId) && int.TryParse(userCompanyId, out var parsedId))
            {
                companyId = parsedId;
            }
        }

        var kpiData = await _dashboardService.GetKpiAsync(startDate, endDate, companyId);
        return Json(kpiData);
    }

    /// <summary>
    /// Grafikler ve Görselleştirmeler Sayfası
    /// </summary>
    public async Task<IActionResult> Charts()
    {
        var isSuperAdmin = User.IsInRole("SuperAdmin");
        int? companyId = null;

        if (!isSuperAdmin)
        {
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            if (!string.IsNullOrEmpty(companyIdClaim) && int.TryParse(companyIdClaim, out var parsedCompanyId))
            {
                companyId = parsedCompanyId;
            }
        }

        var viewModel = new ChartsViewModel
        {
            KpiData = await _dashboardService.GetKpiAsync(companyId: companyId),
            Companies = new List<CompanySelectVm>()
        };

        // SuperAdmin için şirket listesini getir
        if (isSuperAdmin)
        {
            try
            {
                var companies = await _companyService.GetAllAsync();
                viewModel.Companies = companies?.Select(c => new CompanySelectVm
                {
                    Id = c.Id,
                    Name = c.Name
                }).ToList() ?? new List<CompanySelectVm>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Şirket listesi alınamadı: {Message}", ex.Message);
            }
        }

        return View(viewModel);
    }

    #region AJAX Endpoints for Charts

    /// <summary>
    /// Satış trendi verilerini getirir (Line Chart)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetSalesTrend(DateTime? startDate, DateTime? endDate, int? companyId)
    {
        companyId = ResolveCompanyId(companyId);
        var kpiData = await _dashboardService.GetKpiAsync(startDate, endDate, companyId);

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
        var kpiData = await _dashboardService.GetKpiAsync(startDate, endDate, companyId);

        return Json(kpiData?.CategorySales?.Select(c => new
        {
            categoryId = c.CategoryId,
            name = c.CategoryName,
            sales = c.TotalSales,
            quantity = c.TotalQuantity,
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
        var kpiData = await _dashboardService.GetKpiAsync(startDate, endDate, companyId);

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
        var kpiData = await _dashboardService.GetKpiAsync(startDate, endDate, companyId);

        return Json(kpiData?.OrderStatusDistribution?.Select(d => new
        {
            date = d.Date.ToString("dd MMM"),
            pending = d.PendingCount,
            shipped = d.ShippedCount,
            delivered = d.DeliveredCount,
            returned = d.ReturnedCount,
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
        var kpiData = await _dashboardService.GetKpiAsync(startDate, endDate, companyId);

        return Json(kpiData?.GeographicDistribution?.Select(g => new
        {
            city = g.City,
            state = g.State,
            orderCount = g.OrderCount,
            revenue = g.TotalRevenue,
            percentage = g.Percentage,
            intensity = g.Intensity
        }) ?? Enumerable.Empty<object>());
    }

    /// <summary>
    /// Ortalama sepet tutarı trendini getirir (Line Chart)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAverageCartTrend(DateTime? startDate, DateTime? endDate, int? companyId)
    {
        companyId = ResolveCompanyId(companyId);
        var kpiData = await _dashboardService.GetKpiAsync(startDate, endDate, companyId);

        return Json(kpiData?.AverageCartTrend?.Select(a => new
        {
            date = a.Date.ToString("dd MMM"),
            value = a.AverageCartValue,
            orders = a.OrderCount
        }) ?? Enumerable.Empty<object>());
    }

    /// <summary>
    /// En çok satan ürünleri getirir (Horizontal Bar Chart)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetTopProducts(DateTime? startDate, DateTime? endDate, int? companyId)
    {
        companyId = ResolveCompanyId(companyId);
        var kpiData = await _dashboardService.GetKpiAsync(startDate, endDate, companyId);

        return Json(kpiData?.TopProducts?.Select(p => new
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
        var campaigns = await _campaignService.GetActiveAsync();
        return Json(campaigns ?? new List<CampaignVm>());
    }

    /// <summary>
    /// Kampanya özeti getirir
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCampaignSummary()
    {
        var summary = await _campaignService.GetSummaryAsync();
        return Json(summary ?? new CampaignSummaryVm());
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
            var result = await _campaignService.CreateAsync(model);
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
        var result = await _campaignService.DeactivateAsync(id);
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
            var orders = await _orderService.GetAllAsync();
            var order = orders?.FirstOrDefault(o => o.Id == orderId);

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

            var result = await _orderService.UpdateStatusAsync(model.OrderId, status);
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
