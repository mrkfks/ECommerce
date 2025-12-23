using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Services
{
    public class BrandService : IBrandService
    {
        private readonly AppDbContext _context;

        public BrandService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<BrandDto>> GetAllAsync()
        {
            var brands = await _context.Brands.ToListAsync();
            return brands.Select(MapToDto).ToList();
        }

        public async Task<BrandDto?> GetByIdAsync(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            return brand == null ? null : MapToDto(brand);
        }

        public async Task<BrandDto> CreateAsync(BrandCreateDto dto)
        {
            var brand = new Brand
            {
                Name = dto.Name,
                Description = dto.Description ?? string.Empty,
                IsActive = true
            };
            _context.Brands.Add(brand);
            await _context.SaveChangesAsync();
            return MapToDto(brand);
        }

        public async Task UpdateAsync(BrandUpdateDto dto)
        {
            var brand = await _context.Brands.FindAsync(dto.Id);
            if(brand != null)
            {
                brand.Name = dto.Name;
                brand.Description = dto.Description ?? string.Empty;
                brand.IsActive = dto.IsActive;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if(brand != null)
            {
                _context.Brands.Remove(brand);
                await _context.SaveChangesAsync();
            }
        }

        private static BrandDto MapToDto(Brand b)
        {
            return new BrandDto
            {
                Id = b.Id,
                Name = b.Name,
                Description = b.Description,
                ImageUrl = b.ImageUrl,
                IsActive = b.IsActive
            };
        }
    }
}
