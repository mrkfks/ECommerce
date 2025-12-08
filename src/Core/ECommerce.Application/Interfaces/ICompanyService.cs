namespace ECommerce.Application.Interfaces
{
    public interface ICompanyService
    {
        Task<CompanyDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<CompanyDto>> GetAllAsync();
        Task<CompanyDto> CreateAsync(CompanyCreateDto dto);
        Task UpdateAsync(CompanyUpdateDto dto);
        Task DeleteAsync(int id);

        Task<IReadOnlyList<UserDto>> GetUsersAsync(int companyId);
        Task<IReadOnlyList<CustomerDto>> GetCustomersAsync(int companyId);
    }
}
