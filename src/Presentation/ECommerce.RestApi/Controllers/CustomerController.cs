using ECommerce.Application.DTOs;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.RestApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "SameCompanyOrSuperAdmin")]
public class CustomerController : ControllerBase
{
    private readonly AppDbContext _context;
    
    public CustomerController(AppDbContext context)
    {
        _context = context;
    }
    
    [HttpPost]
    [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
    public async Task<IActionResult> Add(CustomerCreateDto dto)
    {
        var customer = Customer.Create(
            dto.CompanyId,
            dto.FirstName,
            dto.LastName,
            dto.Email,
            dto.PhoneNumber,
            dto.DateOfBirth,
            dto.UserId
        );
        
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        
        return Ok(new { id = customer.Id, message = "Müşteri oluşturuldu" });
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var customers = await _context.Customers
            .AsNoTracking()
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                Name = c.FirstName + " " + c.LastName,
                Email = c.Email,
                PhoneNumber = c.PhoneNumber,
                CompanyId = c.CompanyId,
                DateOfBirth = c.DateOfBirth,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync();
        return Ok(customers);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                Name = c.FirstName + " " + c.LastName,
                Email = c.Email,
                PhoneNumber = c.PhoneNumber,
                CompanyId = c.CompanyId,
                DateOfBirth = c.DateOfBirth,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .FirstOrDefaultAsync();
            
        if (customer == null)
            return NotFound(new { message = "Müşteri Bulunamadı" });
        return Ok(customer);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
    public async Task<IActionResult> Update(int id, CustomerUpdateDto dto)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
            return NotFound(new { message = "Müşteri Bulunamadı" });
            
        customer.Update(dto.FirstName, dto.LastName, dto.Email, dto.PhoneNumber);
        await _context.SaveChangesAsync();
        
        return Ok(new { message = "Müşteri Güncellendi" });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
    public async Task<IActionResult> Delete(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
            return NotFound(new { message = "Müşteri Bulunamadı" });
            
        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();
        
        return Ok(new { message = "Müşteri Silindi" });
    }
}