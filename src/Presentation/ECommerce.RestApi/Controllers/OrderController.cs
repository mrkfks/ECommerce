using ECommerce.Application.DTOs;
using ECommerce.Application.DTOs.Common;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Services; // For explicit casting if needed, but preferably use Interface
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.RestApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "SameCompanyOrSuperAdmin")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    
    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(OrderCreateDto dto)
    {
        try
        {
            var result = await _orderService.CreateAsync(dto);
            return Ok(new { id = result.Id, message = "Sipariş başarıyla oluşturuldu." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
    public async Task<IActionResult> GetAll()
    {
        var orders = await _orderService.GetAllAsync();
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _orderService.GetByIdAsync(id);
        if (order == null)
        {
            return NotFound(new { message = "Sipariş Bulunamadı" });
        }
        return Ok(order);
    }

    [HttpGet("my-orders")]
    public async Task<IActionResult> GetMyOrders()
    {
        var userId = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Kullanıcı kimliği bulunamadı" });
        }

        var orders = await _orderService.GetMyOrdersAsync(int.Parse(userId));
        return Ok(new ApiResponseDto<IReadOnlyList<OrderDto>>
        {
            Success = true,
            Data = orders,
            Message = "Siparişler başarıyla getirildi"
        });
    }

    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetByCustomer(int customerId)
    {
        var orders = await _orderService.GetByCustomerIdAsync(customerId);
        return Ok(orders);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusDto dto)
    {
        try
        {
            await _orderService.UpdateStatusAsync(id, dto.Status);
            return Ok(new { message = "Sipariş Durumu Güncellendi" });
        }
        catch (Exception ex)
        {
             return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _orderService.DeleteAsync(id);
            return Ok(new { message = "Sipariş Silindi" });
        }
        catch(Exception ex)
        {
             return NotFound(new { message = ex.Message });
        }
    }
}