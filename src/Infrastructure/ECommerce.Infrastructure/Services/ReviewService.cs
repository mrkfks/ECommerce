using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Services
{
    public class ReviewService : IReviewService
    {
        private readonly AppDbContext _context;

        public ReviewService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ReviewDto?> GetByIdAsync(int id)
        {
            var review = await _context.Reviews
                .Include(r => r.Product)
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.Id == id);
            return review == null ? null : MapToDto(review);
        }
        
        // Controller Compat
        public async Task<ReviewDto?> GetReviewByIdAsync(int id) => await GetByIdAsync(id);

        public async Task<IReadOnlyList<ReviewDto>> GetAllAsync()
        {
            var reviews = await _context.Reviews
                 .Include(r => r.Product)
                 .Include(r => r.Customer)
                 .ToListAsync();
            return reviews.Select(MapToDto).ToList();
        }

        public async Task<IReadOnlyList<ReviewDto>> GetByProductIdAsync(int productId)
        {
             var reviews = await _context.Reviews
                 .Where(r => r.ProductId == productId)
                 .Include(r => r.Product)
                 .Include(r => r.Customer)
                 .ToListAsync();
            return reviews.Select(MapToDto).ToList();
        }
        // Controller Compat
        public async Task<IReadOnlyList<ReviewDto>> GetReviewsByProductIdAsync(int productId) => await GetByProductIdAsync(productId);

        public async Task<IReadOnlyList<ReviewDto>> GetByCustomerIdAsync(int customerId)
        {
             var reviews = await _context.Reviews
                 .Where(r => r.CustomerId == customerId)
                 .Include(r => r.Product)
                 .Include(r => r.Customer)
                 .ToListAsync();
            return reviews.Select(MapToDto).ToList();
        }
        
        public async Task<IReadOnlyList<ReviewDto>> GetByCustomerAsync(int customerId) => await GetByCustomerIdAsync(customerId);

        public async Task<ReviewDto> CreateAsync(ReviewCreateDto dto)
        {
            var review = new Review
            {
                ProductId = dto.ProductId,
                CustomerId = dto.CustomerId,
                ReviewerName = dto.ReviewerName ?? "Anonymous",
                Rating = dto.Rating,
                Comment = dto.Comment,
                CompanyId = dto.CompanyId,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
            return MapToDto(review);
        }
        
        // Controller Compat: AddReviewAsync(int productId, int customerId, int rating, string comment)
        public async Task<ReviewDto> AddReviewAsync(int productId, int customerId, int rating, string comment)
        {
             var dto = new ReviewCreateDto { ProductId = productId, CustomerId = customerId, Rating = rating, Comment = comment };
             return await CreateAsync(dto);
        }

        public async Task UpdateAsync(ReviewUpdateDto dto)
        {
             var review = await _context.Reviews.FindAsync(dto.Id);
             if(review != null)
             {
                 review.Rating = dto.Rating;
                 review.Comment = dto.Comment;
                 await _context.SaveChangesAsync();
             }
        }
        
        // Controller Compat
         public async Task<ReviewDto?> UpdateAsync(int id, int productId, int customerId, int rating, string comment)
         {
             var review = await _context.Reviews.FindAsync(id);
             if(review == null) return null;
             
             review.Rating = rating;
             review.Comment = comment;
             await _context.SaveChangesAsync();
             return MapToDto(review);
         }


        public async Task DeleteAsync(int id)
        {
             var review = await _context.Reviews.FindAsync(id);
             if(review != null)
             {
                 _context.Reviews.Remove(review);
                 await _context.SaveChangesAsync();
             }
        }
        
        // Controller Compat returning bool
        public async Task<bool> DeleteAsync(int id, bool returnBool)
        {
             var review = await _context.Reviews.FindAsync(id);
             if(review != null)
             {
                 _context.Reviews.Remove(review);
                 await _context.SaveChangesAsync();
                 return true;
             }
             return false;
        }

        public async Task<double> GetAverageRatingAsync(int productId)
        {
            var query = _context.Reviews.Where(r => r.ProductId == productId);
            if(await query.AnyAsync())
            {
                return await query.AverageAsync(r => r.Rating);
            }
            return 0;
        }

        private static ReviewDto MapToDto(Review r)
        {
            return new ReviewDto
            {
                Id = r.Id,
                ProductId = r.ProductId,
                CustomerId = r.CustomerId,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            };
        }
    }
}
