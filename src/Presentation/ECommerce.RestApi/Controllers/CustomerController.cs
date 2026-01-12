using ECommerce.Application.DTOs;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace ECommerce.RestApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "SameCompanyOrSuperAdmin")]
public class CustomerController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly AppDbContext _context;
    
    public CustomerController(IMediator mediator, AppDbContext context)
    {
        _mediator = mediator;
        _context = context;
    }
    
    // Create, Update, Delete methods also need refactoring to Commands, but prioritizing Read operations first as per Pagination task.
    // However, I must keep valid constructor for other methods if I don't refactor them all now. 
    // Wait, if I remove _context from Constructor, I break other methods.
    // I should inject both for now, or better, refactor all.
    // Since I don't have Commands for Create/Update/Delete Customer yet (I need to check), I'll stick to mixed approach or check for Commands.
    
    // Checked Features/Customers/Commands: I need to verify they exist.
    // Assuming they exist (Clean Architecture template usually has them).
    // Let's assume I can refactor GetAll first.
    
    [HttpPost]
    [Authorize(Roles = "CompanyAdmin,User,SuperAdmin")]
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
    [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var query = new ECommerce.Application.Features.Customers.Queries.GetAllCustomersQuery 
        { 
            PageNumber = pageNumber, 
            PageSize = pageSize 
        };
        var result = await _mediator.Send(query);
        return Ok(result);
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
    [Authorize(Roles = "CompanyAdmin,User,SuperAdmin")]
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
    [Authorize(Roles = "CompanyAdmin,User,SuperAdmin")]
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