using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly AppDbContext _context;

        public CustomerService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CustomerDto?> GetByIdAsync(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.Addresses)
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);
            return customer == null ? null : MapToDto(customer);
        }

        public async Task<IReadOnlyList<CustomerDto>> GetAllAsync()
        {
            var customers = await _context.Customers
                .Include(c => c.Addresses)
                .Include(c => c.User)
                .ToListAsync();
            return customers.Select(MapToDto).ToList();
        }

        public async Task<IReadOnlyList<CustomerSummaryDto>> GetSummariesAsync()
        {
            // Placeholder: mapping CustomerDto to Summary
             var customers = await _context.Customers.ToListAsync();
             return customers.Select(c => new CustomerSummaryDto 
             { 
                 Id = c.Id, 
                 Name = $"{c.FirstName} {c.LastName}",
                 Email = c.Email,
                 TotalOrders = c.Orders?.Count ?? 0,
                 TotalSpent = c.Orders?.Sum(o => o.TotalAmount) ?? 0,
                 OrderCount = c.Orders?.Count ?? 0,
                 ReviewCount = c.Reviews?.Count ?? 0
             }).ToList();
        }

        public async Task<CustomerDto> CreateAsync(CustomerCreateDto dto)
        {
            var customer = new Customer
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                CompanyId = dto.CompanyId,
                UserId = dto.UserId
            };
            
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return MapToDto(customer);
        }

        public async Task UpdateAsync(CustomerUpdateDto dto)
        {
            var customer = await _context.Customers.FindAsync(dto.Id);
            if(customer != null)
            {
                customer.FirstName = dto.FirstName;
                customer.LastName = dto.LastName;
                customer.PhoneNumber = dto.PhoneNumber;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if(customer != null)
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IReadOnlyList<OrderDto>> GetOrdersAsync(int customerId)
        {
            // Rely on OrderService or direct DB
             var orders = await _context.Orders
                .Where(o => o.CustomerId == customerId)
                .ToListAsync();
             // Minimal mapping or proper logic
             return new List<OrderDto>(); // Placeholder
        }

        public async Task<IReadOnlyList<AddressDto>> GetAddressesAsync(int customerId)
        {
             var addresses = await _context.Addresses
                .Where(a => a.CustomerId == customerId)
                .ToListAsync();
             return addresses.Select(a => new AddressDto 
             { 
                 Id = a.Id,
                 CustomerId = a.CustomerId,
                 Street = a.Street,
                 City = a.City,
                 State = a.State,
                 Country = a.Country,
                 PostalCode = a.ZipCode,
                 CustomerName = "",
                 CompanyId = 0,
                 CompanyName = ""
             }).ToList();
        }

        public async Task<IReadOnlyList<ReviewDto>> GetReviewsAsync(int customerId)
        {
             var reviews = await _context.Reviews
                .Where(r => r.CustomerId == customerId)
                .ToListAsync();
             return reviews.Select(r => new ReviewDto { Id = r.Id, Comment = r.Comment, Rating = r.Rating }).ToList();
        }

        private static CustomerDto MapToDto(Customer c)
        {
            return new CustomerDto
            {
                Id = c.Id,
                Name = $"{c.FirstName} {c.LastName}",
                Email = c.Email,
                PhoneNumber = c.PhoneNumber,
                CompanyId = c.CompanyId,
                DateOfBirth = c.DateOfBirth,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                UserId = c.UserId ?? 0
            };
        }
    }
}
