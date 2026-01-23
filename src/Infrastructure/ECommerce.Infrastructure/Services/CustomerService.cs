using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Application.DTOs.Common;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly AppDbContext _context;
        private readonly ITenantService _tenantService;
        private readonly ILogger<CustomerService> _logger;
        private readonly IMapper _mapper;

        public CustomerService(AppDbContext context, ITenantService tenantService, ILogger<CustomerService> logger, IMapper mapper)
        {
            _context = context;
            _tenantService = tenantService;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<CustomerDto> CreateAsync(CustomerCreateDto dto)
        {
            try
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

                return MapToDto(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer");
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
                throw new Exception("Müşteri Bulunamadı");

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<CustomerDto>> GetAllAsync()
        {
            var companyId = _tenantService.GetCompanyId();
            var isSuperAdmin = _tenantService.IsSuperAdmin();

            var query = _context.Customers.AsNoTracking();

            if (!isSuperAdmin && companyId.HasValue)
            {
                query = query.Where(c => c.CompanyId == companyId.Value);
            }

            var customers = await query.OrderBy(c => c.FirstName).ToListAsync();

            return customers.Select(MapToDto).ToList();
        }

        public async Task<IReadOnlyList<AddressDto>> GetAddressesAsync(int customerId)
        {
            var addresses = await _context.Set<Address>()
                .Where(a => a.CustomerId == customerId)
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<List<AddressDto>>(addresses);
        }

        public async Task<CustomerDto?> GetByIdAsync(int id)
        {
            var customer = await _context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            return customer == null ? null : MapToDto(customer);
        }

        public async Task<IReadOnlyList<OrderDto>> GetOrdersAsync(int customerId)
        {
            // Simple mapping for now, ideally delegate to OrderService or Repository
            var orders = await _context.Orders
               .Include(o => o.Company)
               .Where(o => o.CustomerId == customerId)
               .AsNoTracking()
               .ToListAsync();

            // Manual mapping as OrderDto is complex
            return orders.Select(o => new OrderDto
            {
                Id = o.Id,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                StatusText = o.Status.ToString(),
                CompanyId = o.CompanyId,
                CompanyName = o.Company?.Name ?? string.Empty,
                CustomerId = o.CustomerId
            }).ToList();
        }

        public Task<IReadOnlyList<ReviewDto>> GetReviewsAsync(int customerId)
        {
            // Not implemented yet
            return Task.FromResult<IReadOnlyList<ReviewDto>>(new List<ReviewDto>());
        }

        public async Task<IReadOnlyList<CustomerSummaryDto>> GetSummariesAsync()
        {
            var companyId = _tenantService.GetCompanyId();
            var query = _context.Customers.AsNoTracking();
            if (companyId.HasValue) query = query.Where(c => c.CompanyId == companyId.Value);

            var customers = await query
               .Select(c => new CustomerSummaryDto
               {
                   Id = c.Id,
                   Name = c.FirstName + " " + c.LastName,
                   Email = c.Email,
                   PhoneNumber = c.PhoneNumber
               })
               .ToListAsync();
            return customers;
        }

        public async Task UpdateAsync(CustomerUpdateDto dto)
        {
            var customer = await _context.Customers.FindAsync(dto.Id);
            if (customer == null)
                throw new Exception("Müşteri Bulunamadı");

            customer.Update(dto.FirstName, dto.LastName, dto.Email, dto.PhoneNumber);
            await _context.SaveChangesAsync();
        }

        private static CustomerDto MapToDto(Customer c)
        {
            return new CustomerDto
            {
                Id = c.Id,
                Name = c.FirstName + " " + c.LastName,
                Email = c.Email,
                PhoneNumber = c.PhoneNumber,
                CompanyId = c.CompanyId,
                DateOfBirth = c.DateOfBirth,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            };
        }
    }
}
