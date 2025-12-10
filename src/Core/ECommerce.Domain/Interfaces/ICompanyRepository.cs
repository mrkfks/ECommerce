using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Interfaces
{
    public interface ICompanyRepository : IGenericRepository<Company>
    {
        Task<Company?> GetByTaxNumberAsync(string taxNumber);
        Task<Company?> GetByEmailAsync(string email);
        Task<IReadOnlyList<Company>> GetActiveCompaniesAsync();
    }
}
