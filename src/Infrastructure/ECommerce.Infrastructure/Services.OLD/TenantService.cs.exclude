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
        if (user == null || !user.Identity.IsAuthenticated) return null;

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
}
