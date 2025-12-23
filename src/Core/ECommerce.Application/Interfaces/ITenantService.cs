namespace ECommerce.Application.Interfaces;

public interface ITenantService
{
    int? GetCompanyId();
    int GetCurrentCompanyId();
    bool IsSuperAdmin();
}
