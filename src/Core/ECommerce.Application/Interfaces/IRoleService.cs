using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces;

public interface IRoleService
{
    Task<IReadOnlyList<RoleDto>> GetAllAsync();
    Task<RoleDto?> GetByIdAsync(int id);
}
