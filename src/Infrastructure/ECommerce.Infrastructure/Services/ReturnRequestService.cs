using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services
{
    public class ReturnRequestService : IReturnRequestService
    {
        private readonly AppDbContext _context;
        private readonly ITenantService _tenantService;
        private readonly ILogger<ReturnRequestService> _logger;

        public ReturnRequestService(
            AppDbContext context,
            ITenantService tenantService,
            ILogger<ReturnRequestService> logger)
        {
            _context = context;
            _tenantService = tenantService;
            _logger = logger;
        }

        public async Task<ReturnRequestDto?> GetByIdAsync(int id)
        {
            var returnRequest = await _context.ReturnRequests
                .Include(r => r.Product)
                .Include(r => r.Customer)
                .Include(r => r.Order)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (returnRequest == null)
                return null;

            return MapToDto(returnRequest);
        }

        public async Task<IReadOnlyList<ReturnRequestDto>> GetAllAsync()
        {
            var companyId = _tenantService.GetCompanyId();

            var returnRequests = await _context.ReturnRequests
                .Where(r => r.CompanyId == companyId)
                .Include(r => r.Product)
                .Include(r => r.Customer)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            return returnRequests.Select(MapToDto).ToList();
        }

        public async Task<IReadOnlyList<ReturnRequestDto>> GetByOrderIdAsync(int orderId)
        {
            var returnRequests = await _context.ReturnRequests
                .Where(r => r.OrderId == orderId)
                .Include(r => r.Product)
                .Include(r => r.Customer)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            return returnRequests.Select(MapToDto).ToList();
        }

        public async Task<IReadOnlyList<ReturnRequestDto>> GetByCustomerIdAsync(int customerId)
        {
            var returnRequests = await _context.ReturnRequests
                .Where(r => r.CustomerId == customerId)
                .Include(r => r.Product)
                .Include(r => r.Customer)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            return returnRequests.Select(MapToDto).ToList();
        }

        public async Task<IReadOnlyList<ReturnRequestDto>> GetMyReturnRequestsAsync(int userId)
        {
            // UserId'den CustomerId'yi bul
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (customer == null)
                return new List<ReturnRequestDto>();

            return await GetByCustomerIdAsync(customer.Id);
        }

        public async Task<ReturnRequestDto> CreateAsync(CreateReturnRequestDto dto, int userId)
        {
            // Müşteri bilgisini al
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (customer == null)
                throw new Exception("Müşteri bulunamadı");

            // Order var mı kontrol et
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == dto.OrderId && o.CustomerId == customer.Id);

            if (order == null)
                throw new Exception("Sipariş bulunamadı");

            // OrderItem var mı kontrol et
            var orderItem = order.Items.FirstOrDefault(oi => oi.Id == dto.OrderItemId);
            if (orderItem == null)
                throw new Exception("Sipariş öğesi bulunamadı");

            // Ürünü kontrol et
            var product = await _context.Products.FindAsync(dto.ProductId);
            if (product == null)
                throw new Exception("Ürün bulunamadı");

            // Aynı sipariş ve ürün için aktif bir iade talebi olup olmadığını kontrol et
            var existingReturnRequest = await _context.ReturnRequests
                .FirstOrDefaultAsync(r => r.OrderId == dto.OrderId &&
                                          r.ProductId == dto.ProductId &&
                                          r.Status == ReturnRequestStatus.Pending);

            if (existingReturnRequest != null)
                throw new Exception("Bu ürün için zaten beklemede olan bir iade talebi mevcut.");

            // ReturnRequest oluştur
            var returnRequest = ReturnRequest.Create(
                orderId: order.Id,
                orderItemId: orderItem.Id,
                productId: product.Id,
                customerId: customer.Id,
                companyId: order.CompanyId,
                quantity: dto.Quantity,
                reason: dto.Reason,
                comments: dto.Comments);

            _context.ReturnRequests.Add(returnRequest);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"İade talebi oluşturuldu. ID: {returnRequest.Id}, OrderID: {order.Id}");

            return MapToDto(returnRequest);
        }

        public async Task UpdateStatusAsync(int id, UpdateReturnRequestDto dto)
        {
            var returnRequest = await _context.ReturnRequests.FindAsync(id);
            if (returnRequest == null)
                throw new Exception("İade talebi bulunamadı");

            switch (dto.Status)
            {
                case ReturnRequestStatus.Approved:
                    returnRequest.Approve(dto.AdminResponse);
                    break;
                case ReturnRequestStatus.Rejected:
                    returnRequest.Reject(dto.AdminResponse);
                    break;
                case ReturnRequestStatus.Processing:
                    returnRequest.MarkAsProcessing(dto.AdminResponse);
                    break;
                case ReturnRequestStatus.Completed:
                    returnRequest.Complete(dto.AdminResponse);
                    break;
                default:
                    throw new Exception("Geçersiz durum");
            }

            _context.ReturnRequests.Update(returnRequest);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"İade talebi güncellendi. ID: {id}, Durum: {dto.Status}");
        }

        public async Task<IReadOnlyList<ReturnRequestDto>> GetPendingRequestsAsync()
        {
            var companyId = _tenantService.GetCompanyId();

            var returnRequests = await _context.ReturnRequests
                .Where(r => r.CompanyId == companyId && r.Status == ReturnRequestStatus.Pending)
                .Include(r => r.Product)
                .Include(r => r.Customer)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            return returnRequests.Select(MapToDto).ToList();
        }

        private ReturnRequestDto MapToDto(ReturnRequest entity)
        {
            return new ReturnRequestDto
            {
                Id = entity.Id,
                OrderId = entity.OrderId,
                OrderItemId = entity.OrderItemId,
                ProductId = entity.ProductId,
                ProductName = entity.Product?.Name ?? "Bilinmeyen Ürün",
                CustomerId = entity.CustomerId,
                CustomerName = entity.Customer != null ? $"{entity.Customer.FirstName} {entity.Customer.LastName}" : "Bilinmeyen Müşteri",
                CompanyId = entity.CompanyId,
                Quantity = entity.Quantity,
                Reason = entity.Reason,
                Comments = entity.Comments,
                Status = entity.Status,
                StatusText = entity.GetStatusDisplay(),
                AdminResponse = entity.AdminResponse,
                RequestDate = entity.RequestDate,
                ResolutionDate = entity.ResolutionDate
            };
        }
    }
}
