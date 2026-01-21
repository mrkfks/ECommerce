using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Services;

public class RoleService : IRoleService
{
    private readonly AppDbContext _context;

    public RoleService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<RoleDto>> GetAllAsync()
    {
        return await _context.Roles
            .Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name
            })
            .ToListAsync();
    }

    public async Task<RoleDto?> GetByIdAsync(int id)
    {
        return await _context.Roles
            .Where(r => r.Id == id)
            .Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name
            })
            .FirstOrDefaultAsync();
    }
}
