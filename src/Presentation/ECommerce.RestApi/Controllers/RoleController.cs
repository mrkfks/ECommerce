using ECommerce.Application.DTOs;
using ECommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.RestApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoleController : ControllerBase
{
    private readonly AppDbContext _context;

    public RoleController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var roles = await _context.Roles
            .Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name
            })
            .ToListAsync();

        return Ok(roles);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var role = await _context.Roles
            .Where(r => r.Id == id)
            .Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name
            })
            .FirstOrDefaultAsync();

        if (role == null)
            return NotFound();

        return Ok(role);
    }
}
