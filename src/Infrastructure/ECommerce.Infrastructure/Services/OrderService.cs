using ECommerce.Application.DTOs;
using ECommerce.Application.DTOs.Common;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;

namespace ECommerce.Infrastructure.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly ITenantService _tenantService;
        private readonly ILogger<OrderService> _logger;
        private readonly Microsoft.AspNetCore.SignalR.IHubContext<ECommerce.Infrastructure.Hubs.NotificationHub> _hubContext;

        public OrderService(AppDbContext context, ITenantService tenantService, ILogger<OrderService> logger, Microsoft.AspNetCore.SignalR.IHubContext<ECommerce.Infrastructure.Hubs.NotificationHub> hubContext)
        {
            _context = context;
            _tenantService = tenantService;
            _logger = logger;
            _hubContext = hubContext;
        }

        public async Task<OrderDto> CreateAsync(OrderCreateDto dto)
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
                    throw new Exception("Geçerli bir teslimat adresi gereklidir.");
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

                    // Safe Atomic Stock Update (Concurrency Safe)
                    var affectedRows = await _context.Products
                        .Where(p => p.Id == product.Id && p.StockQuantity >= itemDto.Quantity)
                        .ExecuteUpdateAsync(s => s.SetProperty(p => p.StockQuantity, p => p.StockQuantity - itemDto.Quantity));

                    if (affectedRows == 0)
                        throw new Exception($"Yetersiz stok veya eşzamanlı işlem hatası: {product.Name}");
                    
                    // OrderItem oluştur
                    var orderItem = OrderItem.Create(product.Id, itemDto.Quantity, product.Price);
                    order.AddItem(orderItem);
                }

                // 4. Sepeti Temizle (Varsa)
                // Müşterinin bu şirketteki aktif sepetini bul ve temizle
                if (dto.CustomerId > 0)
                {
                     // Sepet kalemlerini sil (CartItem -> Cart -> CustomerId == dto.CustomerId && CompanyId == dto.CompanyId)
                     // Not: CartItem üzerinde doğrudan CustomerId yok, Cart üzerinden gidiyoruz.
                     // Performans için ExecuteDeleteAsync kullanıyoruz.
                     await _context.CartItems
                         .Where(ci => ci.Cart.CustomerId == dto.CustomerId && ci.Cart.CompanyId == dto.CompanyId)
                         .ExecuteDeleteAsync();
                         
                     // Opsiyonel: Sepeti de pasife çekebiliriz veya boş bırakabiliriz.
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // 5. Send Real-Time Notification
                try
                {
                    // Tenant-specific group 'Tenant_{CompanyId}'
                    var notificationMessage = $"Yeni Sipariş Alındı! Sipariş No: {order.Id}, Tutar: {order.TotalAmount:C2}";
                    await _hubContext.Clients.Group($"Tenant_{order.CompanyId}").SendAsync("ReceiveNotification", notificationMessage);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send real-time notification for Order {OrderId}", order.Id);
                    // Do not throw, order is already committed
                }

                return await GetByIdAsync(order.Id) ?? throw new Exception("Order created but not found");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Order creation failed");
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                throw new Exception("Sipariş Bulunamadı");
                
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<OrderDto>> GetAllAsync()
        {
            var companyId = _tenantService.GetCompanyId();
            var orders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Company)
                .Include(o => o.Address)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .AsNoTracking()
                .Where(o => !companyId.HasValue || o.CompanyId == companyId.Value)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => MapToDto(o))
                .ToListAsync();
            
            return orders;
        }

        public async Task<IReadOnlyList<OrderDto>> GetByCustomerIdAsync(int customerId)
        {
             var orders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Company)
                .Include(o => o.Address)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .AsNoTracking()
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => MapToDto(o))
                .ToListAsync();
            return orders;
        }

        public async Task<OrderDto?> GetByIdAsync(int id)
        {
             var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Company)
                .Include(o => o.Address)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .AsNoTracking()
                .Where(o => o.Id == id)
                .FirstOrDefaultAsync();

            return order == null ? null : MapToDto(order);
        }

        public async Task<IReadOnlyList<OrderDto>> GetMyOrdersAsync(int userId)
        {
            var orders = await _context.Orders
             .Include(o => o.Customer)
             .Include(o => o.Company)
             .Include(o => o.Address)
             .Include(o => o.Items)
                 .ThenInclude(i => i.Product)
             .Where(o => o.Customer!.UserId == userId)
             .AsNoTracking()
             .OrderByDescending(o => o.OrderDate)
             .Select(o => MapToDto(o))
             .ToListAsync();
             
             return orders;
        }

        public async Task UpdateAsync(OrderUpdateDto dto)
        {
             // Not implemented in Controller, implementing basic update if needed or empty
             var order = await _context.Orders.FindAsync(dto.Id);
             if (order == null) throw new Exception("Order not found");
             // Logic to update...
             await _context.SaveChangesAsync();
        }

        public async Task UpdateStatusAsync(int orderId, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                throw new Exception("Sipariş Bulunamadı");
                
            switch (status)
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
        }

        public async Task AddItemAsync(int orderId, OrderItemCreateDto itemDto)
        {
             // Placeholder implementation
             await Task.CompletedTask;
        }

        public async Task RemoveItemAsync(int orderId, int productId)
        {
             // Placeholder implementation
             await Task.CompletedTask;
        }

        private static OrderDto MapToDto(Order o)
        {
            return new OrderDto
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
            };
        }
    }
}
