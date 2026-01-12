using ECommerce.Application.DTOs;
using ECommerce.Application.DTOs.Common;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.RestApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "SameCompanyOrSuperAdmin")]
public class OrderController : ControllerBase
{
    private readonly AppDbContext _context;
    
    public OrderController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Create(OrderCreateDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. Adres İşlemleri
            int addressId = dto.AddressId;
            if (addressId <= 0 && dto.ShippingAddress != null)
            {
                var newAddress = Address.Create(
                    dto.CustomerId,
                    dto.ShippingAddress.Street,
                    dto.ShippingAddress.City,
                    dto.ShippingAddress.State,
                    dto.ShippingAddress.ZipCode,
                    dto.ShippingAddress.Country
                );
                _context.Set<Address>().Add(newAddress);
                await _context.SaveChangesAsync();
                addressId = newAddress.Id;
            }

            if (addressId <= 0)
            {
                return BadRequest(new { message = "Geçerli bir teslimat adresi gereklidir." });
            }

            // 2. Sipariş Oluşturma
            var order = Order.Create(dto.CustomerId, addressId, dto.CompanyId);
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // 3. Kalemleri Ekle ve Stok Düş
            foreach (var itemDto in dto.Items)
            {
                var product = await _context.Products.FindAsync(itemDto.ProductId);
                if (product == null)
                    throw new Exception($"Ürün bulunamadı: {itemDto.ProductId}");

                if (!product.IsActive)
                    throw new Exception($"Ürün satışa kapalı: {product.Name}");

                if (product.StockQuantity < itemDto.Quantity)
                    throw new Exception($"Yetersiz stok: {product.Name}");

                // Stok düş
                product.UpdateStock(product.StockQuantity - itemDto.Quantity);
                
                // OrderItem oluştur
                var orderItem = OrderItem.Create(product.Id, itemDto.Quantity, product.Price);
                order.AddItem(orderItem);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { id = order.Id, message = "Sipariş başarıyla oluşturuldu." });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
    public async Task<IActionResult> GetAll()
    {
        var orders = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Company)
            .Include(o => o.Address)
            .Include(o => o.Items)
            .AsNoTracking()
            .Select(o => new OrderDto
            {
                Id = o.Id,
                CustomerId = o.CustomerId,
                CustomerName = o.Customer != null ? o.Customer.FirstName + " " + o.Customer.LastName : "",
                AddressId = o.AddressId,
                CompanyId = o.CompanyId,
                CompanyName = o.Company != null ? o.Company.Name : "",
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                StatusText = o.Status.ToString()
            })
            .ToListAsync();
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Company)
            .Include(o => o.Address)
            .Include(o => o.Items)
            .AsNoTracking()
            .Where(o => o.Id == id)
            .Select(o => new OrderDto
            {
                Id = o.Id,
                CustomerId = o.CustomerId,
                CustomerName = o.Customer != null ? o.Customer.FirstName + " " + o.Customer.LastName : "",
                AddressId = o.AddressId,
                CompanyId = o.CompanyId,
                CompanyName = o.Company != null ? o.Company.Name : "",
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                StatusText = o.Status.ToString()
            })
            .FirstOrDefaultAsync();
            
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

        var orders = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Company)
            .Include(o => o.Address)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .Where(o => o.Customer!.UserId == int.Parse(userId))
            .AsNoTracking()
            .Select(o => new OrderDto
            {
                Id = o.Id,
                CustomerId = o.CustomerId,
                CustomerName = o.Customer != null ? o.Customer.FirstName + " " + o.Customer.LastName : "",
                AddressId = o.AddressId,
                CompanyId = o.CompanyId,
                CompanyName = o.Company != null ? o.Company.Name : "",
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                StatusText = o.Status.ToString(),
                Items = o.Items.Select(i => new OrderItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product != null ? i.Product.Name : "",
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice
                }).ToList()
            })
            .ToListAsync();

        return Ok(new ApiResponseDto<List<OrderDto>>
        {
            Success = true,
            Data = orders,
            Message = "Siparişler başarıyla getirildi"
        });
    }

    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetByCustomer(int customerId)
    {
        var orders = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Company)
            .Where(o => o.CustomerId == customerId)
            .AsNoTracking()
            .Select(o => new OrderDto
            {
                Id = o.Id,
                CustomerId = o.CustomerId,
                CustomerName = o.Customer != null ? o.Customer.FirstName + " " + o.Customer.LastName : "",
                AddressId = o.AddressId,
                CompanyId = o.CompanyId,
                CompanyName = o.Company != null ? o.Company.Name : "",
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                StatusText = o.Status.ToString()
            })
            .ToListAsync();
        return Ok(orders);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusDto dto)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
            return NotFound(new { message = "Sipariş Bulunamadı" });
            
        switch (dto.Status)
        {
            case OrderStatus.Processing:
                order.Confirm();
                break;
            case OrderStatus.Shipped:
                order.Ship();
                break;
            case OrderStatus.Delivered:
                order.Deliver();
                break;
            case OrderStatus.Cancelled:
                order.Cancel();
                break;
        }
        
        await _context.SaveChangesAsync();
        return Ok(new { message = "Sipariş Durumu Güncellendi" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
            return NotFound(new { message = "Sipariş Bulunamadı" });
            
        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();
        
        return Ok(new { message = "Sipariş Silindi" });
    }
}