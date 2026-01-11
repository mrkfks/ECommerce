using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace ECommerce.Infrastructure.Services;

public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<TenantService> _logger;

    public TenantService(IHttpContextAccessor httpContextAccessor, ILogger<TenantService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public int? GetCompanyId()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            _logger.LogWarning("HttpContext is null in TenantService");
            return null;
        }

        if (context.Items.TryGetValue("CompanyId", out var itemCompany) && itemCompany is int companyFromItems)
        {
            _logger.LogInformation("CompanyId found in HttpContext.Items: {CompanyId}", companyFromItems);
            return companyFromItems;
        }

        var user = context.User;
        var isAuthenticated = user?.Identity?.IsAuthenticated == true;

        // 1) Eğer header'da şirket kimliği varsa, bunu öncelikli kullan
        // Bu sayede anonim ziyaretçiler için de tenant bağlamı sağlanır
        if (context.Request?.Headers != null && context.Request.Headers.TryGetValue("X-Company-Id", out var headerValues))
        {
            var headerVal = headerValues.ToString();
            _logger.LogInformation("X-Company-Id header found: '{HeaderValue}'", headerVal);
            if (!string.IsNullOrWhiteSpace(headerVal) && int.TryParse(headerVal, out int headerCompanyId))
            {
                _logger.LogInformation("Returning CompanyId from header: {CompanyId}", headerCompanyId);
                return headerCompanyId;
            }
        }
        else
        {
            _logger.LogWarning("X-Company-Id header NOT found");
        }

        if (!isAuthenticated)
        {
            _logger.LogInformation("User not authenticated, returning null");
            return null;
        }

        if (user == null)
        {
            _logger.LogWarning("User principal is null in TenantService");
            return null;
        }

        // SuperAdmin ise her şeyi görebilsin (null dönerek filtreyi devre dışı bırak)
        if (user.IsInRole("SuperAdmin"))
        {
            return null;
        }

        // "CompanyId" claim'ini ara
        var companyClaim = user.FindFirst("CompanyId")?.Value;
        if (!string.IsNullOrEmpty(companyClaim) && int.TryParse(companyClaim, out int companyId))
        {
            return companyId;
        }

        return null;
    }

    public int GetCurrentCompanyId()
    {
        var companyId = GetCompanyId();
        if (companyId.HasValue)
            return companyId.Value;
        
        // Eğer null ise (SuperAdmin gibi), varsayılan olarak 1 dön (veya hata fırlat)
        return 1;
    }

    public bool IsSuperAdmin()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return false;

        var user = context.User;
        if (user == null || !user.Identity?.IsAuthenticated == true) return false;

        return user.IsInRole("SuperAdmin");
    }
}
