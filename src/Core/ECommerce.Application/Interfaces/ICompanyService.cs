using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces;

public interface ICompanyService
{
    Task<CompanyRegistrationResultDto> RegisterCompanyAsync(RegisterDto dto);
    Task<CompanyDto> CreateAsync(CompanyFormDto dto);
    Task<IReadOnlyList<CompanyDto>> GetAllAsync();
    Task<CompanyDto?> GetByIdAsync(int id);
    Task UpdateAsync(int id, CompanyFormDto dto);
    Task ApproveAsync(int id);
    Task RejectAsync(int id);
    Task ActivateAsync(int id);
    Task DeactivateAsync(int id);
    Task<CompanyDto?> GetByDomainAsync(string domain);
    Task UpdateBrandingAsync(int id, object dto); // Receiving DTO from controller (using dynamic/object for now to avoid compilation error if DTO not moved)
}
