using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.RestApi.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize(Policy = "SameCompanyOrSuperAdmin")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;
    
    public CustomerController(ICustomerService customerService)
    {
        _customerService = customerService;
    }
    
    [HttpPost]
    [Authorize(Roles = "CompanyAdmin,User,SuperAdmin")]
    public async Task<IActionResult> Add(CustomerCreateDto dto)
    {
        try
        {
            var customer = await _customerService.CreateAsync(dto);
            return Ok(new { id = customer.Id, message = "Müşteri oluşturuldu" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        // TODO: Pagination support in Service
        var customers = await _customerService.GetAllAsync();
        // Simple pagination for now
        var paged = customers.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        
        return Ok(new 
        { 
            Data = paged,
            TotalCount = customers.Count,
            PageNumber = pageNumber,
            PageSize = pageSize
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var customer = await _customerService.GetByIdAsync(id);
        if (customer == null)
            return NotFound(new { message = "Müşteri Bulunamadı" });
        return Ok(customer);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "CompanyAdmin,User,SuperAdmin")]
    public async Task<IActionResult> Update(int id, CustomerUpdateDto dto)
    {
        if (id != dto.Id) return BadRequest("ID mismatch");
        try
        {
            await _customerService.UpdateAsync(dto);
            return Ok(new { message = "Müşteri Güncellendi" });
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "CompanyAdmin,User,SuperAdmin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _customerService.DeleteAsync(id);
            return Ok(new { message = "Müşteri Silindi" });
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}