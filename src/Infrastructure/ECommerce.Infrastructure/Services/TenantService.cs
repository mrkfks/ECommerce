using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ECommerce.Infrastructure.Services;

public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? GetCompanyId()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return null;

        var user = context.User;
        if (user == null || !user.Identity?.IsAuthenticated == true) return null;

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
