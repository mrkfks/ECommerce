using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.RestApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;
    
    public CustomerController(ICustomerService customerService)
    {
        _customerService = customerService;
    }
    
    [HttpPost]
    public async Task<IActionResult> Add(CustomerCreateDto dto)
    {
        var customer = await _customerService.CreateAsync(dto);
        return Ok(customer);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var customers = await _customerService.GetAllAsync();
        return Ok(customers);
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
    public async Task<IActionResult> Update(int id, CustomerUpdateDto dto)
    {
        dto.Id = id;
        await _customerService.UpdateAsync(dto);
        return Ok(new { message = "Müşteri Güncellendi" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _customerService.DeleteAsync(id);
        return Ok(new { message = "Müşteri Silindi" });
    }
}