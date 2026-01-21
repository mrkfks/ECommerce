using System.Security.Claims;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

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
        if (context == null) return null;

        var user = context.User;
        var isAuthenticated = user?.Identity?.IsAuthenticated == true;

        // 1. JWT Claim'den TenantId (CompanyId) Okuma - Öncelikli
        if (isAuthenticated)
        {
            // İstenen özel claim veya standart primarysid
            var tenantClaim = user.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/primarysid") 
                              ?? user.FindFirst("TenantId")
                              ?? user.FindFirst("CompanyId");

            if (tenantClaim != null && int.TryParse(tenantClaim.Value, out int claimCompanyId))
            {
                return claimCompanyId;
            }
        }

        // 2. HTTP Header'dan Okuma (Fallback veya Anonim testler için)
        if (context.Request.Headers.TryGetValue("X-Company-Id", out var headerValue))
        {
            if (int.TryParse(headerValue.ToString(), out int headerCompanyId))
            {
                return headerCompanyId;
            }
        }

        return null;
    }

    public int GetCurrentCompanyId()
    {
        return GetCompanyId() ?? 0; // 0 veya varsayılan değer
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
