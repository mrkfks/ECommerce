using ECommerce.Application.DTOs;

namespace Dashboard.Web.Services.Contracts;

/// <summary>
/// Sipariş yönetimi için servis arayüzü
/// </summary>
public interface IOrderApiService
{
    Task<IEnumerable<OrderDto>> GetAllAsync();
    Task<OrderDto?> GetByIdAsync(int id);
    Task<IEnumerable<OrderDto>> GetByCustomerAsync(int customerId);
    Task<bool> UpdateStatusAsync(int id, string status);
    Task<bool> CancelAsync(int id);
}
