using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Interfaces
{
    public interface ICustomerRepository : IGenericRepository<Customer>
    {
        Task<Customer?> GetByEmailAsync(string email);
        Task<IReadOnlyList<Customer>> GetWithOrdersAsync();
    }
}