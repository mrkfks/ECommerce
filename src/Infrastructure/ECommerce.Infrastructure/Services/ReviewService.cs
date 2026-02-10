using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services;

public class ReviewService : IReviewService
{
    private readonly AppDbContext _context;
    private readonly ITenantService _tenantService;
    private readonly IMapper _mapper; // Optional if manual mapping used as in Controller
    private readonly ILogger<ReviewService> _logger;

    public ReviewService(AppDbContext context, ITenantService tenantService, IMapper mapper, ILogger<ReviewService> logger)
    {
        _context = context;
        _tenantService = tenantService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ReviewDto?> GetByIdAsync(int id)
    {
        var review = await _context.Reviews
            .Include(r => r.Product)
            .Include(r => r.Customer)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);

        return review == null ? null : MapToDto(review);
    }

    public async Task<IReadOnlyList<ReviewDto>> GetAllAsync()
    {
        // Reviews might be tenant specific?
        // Logic in controller didn't filter by tenant but Review entity might.
        // Review entity in Step 409 shows companyId in Create but not much filtering logic.
        // Assuming global read for now or filter by tenant if ITenantEntity

        // Review created with CompanyId in Create method in Controller.
        // Check Review entity to be sure. 
        // Assuming ITenantEntity or similar.

        var query = _context.Reviews
            .Include(r => r.Product)
            .Include(r => r.Customer)
            .AsNoTracking();

        var companyId = _tenantService.GetCompanyId();
        if (companyId.HasValue) query = query.Where(r => r.CompanyId == companyId.Value);

        var reviews = await query.ToListAsync();
        return reviews.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<ReviewDto>> GetByProductIdAsync(int productId)
    {
        var query = _context.Reviews
            .Include(r => r.Product)
            .Include(r => r.Customer)
            .Where(r => r.ProductId == productId)
            .AsNoTracking();

        var companyId = _tenantService.GetCompanyId();
        if (companyId.HasValue) query = query.Where(r => r.CompanyId == companyId.Value);

        var reviews = await query.ToListAsync();
        return reviews.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<ReviewDto>> GetByCustomerIdAsync(int customerId)
    {
        var query = _context.Reviews
            .Include(r => r.Product)
            .Include(r => r.Customer)
            .Where(r => r.CustomerId == customerId)
            .AsNoTracking();

        var companyId = _tenantService.GetCompanyId();
        if (companyId.HasValue) query = query.Where(r => r.CompanyId == companyId.Value);

        var reviews = await query.ToListAsync();
        return reviews.Select(MapToDto).ToList();
    }

    public async Task<ReviewDto> AddAsync(ReviewFormDto dto)
    {
        var companyId = _tenantService.GetCompanyId() ?? dto.CompanyId;
        // If companyId is still 0/null? Controller DTO has CompanyId.

        var review = Review.Create(
            dto.ProductId,
            dto.CustomerId,
            companyId,
            dto.ReviewerName ?? "Anonim",
            dto.Rating,
            dto.Comment
        );

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        return MapToDto(review);
    }

    public async Task UpdateAsync(int id, ReviewFormDto dto)
    {
        var review = await _context.Reviews.FindAsync(id);
        if (review == null) throw new KeyNotFoundException("Review not found");

        // Auth check? Service layer usually trusts caller or checks ownership.

        review.Update(dto.Rating, dto.Comment);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var review = await _context.Reviews.FindAsync(id);
        if (review == null) throw new KeyNotFoundException("Review not found");

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();
    }

    private static ReviewDto MapToDto(Review r)
    {
        return new ReviewDto
        {
            Id = r.Id,
            ReviewerName = r.ReviewerName,
            Rating = r.Rating,
            Comment = r.Comment,
            CreatedAt = r.CreatedAt,
            ProductId = r.ProductId,
            ProductName = r.Product != null ? r.Product.Name : "",
            CustomerId = r.CustomerId,
            CustomerName = r.Customer != null ? r.Customer.FirstName + " " + r.Customer.LastName : ""
        };
    }

    public async Task<ReviewSummaryDto> GetProductSummaryAsync(int productId)
    {
        var reviews = await _context.Reviews
            .Where(r => r.ProductId == productId)
            .AsNoTracking()
            .ToListAsync();

        if (!reviews.Any())
        {
            return new ReviewSummaryDto
            {
                ProductId = productId,
                AverageRating = 0,
                TotalReviews = 0,
                RatingDistribution = new Dictionary<int, int>
                {
                    { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 }
                }
            };
        }

        var averageRating = reviews.Average(r => r.Rating);
        var ratingDistribution = reviews
            .GroupBy(r => r.Rating)
            .ToDictionary(g => g.Key, g => g.Count());

        // Ensure all ratings 1-5 are present
        for (int i = 1; i <= 5; i++)
        {
            if (!ratingDistribution.ContainsKey(i))
                ratingDistribution[i] = 0;
        }

        return new ReviewSummaryDto
        {
            ProductId = productId,
            AverageRating = Math.Round(averageRating, 1),
            TotalReviews = reviews.Count,
            RatingDistribution = ratingDistribution
        };
    }

    public async Task<IReadOnlyList<ReviewDto>> GetMyReviewsAsync(int userId)
    {
        // Kullanıcının CustomerId'sini bul
        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (customer == null)
            return new List<ReviewDto>();

        var reviews = await _context.Reviews
            .Include(r => r.Product)
            .Include(r => r.Customer)
            .Where(r => r.CustomerId == customer.Id)
            .AsNoTracking()
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return reviews.Select(MapToDto).ToList();
    }

    public async Task<CanReviewDto> CanReviewAsync(int productId, int userId)
    {
        // Kullanıcının CustomerId'sini bul
        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (customer == null)
        {
            return new CanReviewDto
            {
                CanReview = false,
                HasPurchased = false,
                HasReviewed = false
            };
        }

        // Bu ürünü satın almış mı?
        var hasPurchased = await _context.OrderItems
            .AnyAsync(oi => oi.ProductId == productId &&
                           oi.Order != null &&
                           oi.Order.CustomerId == customer.Id);

        // Bu ürüne daha önce yorum yapmış mı?
        var hasReviewed = await _context.Reviews
            .AnyAsync(r => r.ProductId == productId && r.CustomerId == customer.Id);

        return new CanReviewDto
        {
            CanReview = hasPurchased && !hasReviewed,
            HasPurchased = hasPurchased,
            HasReviewed = hasReviewed
        };
    }
}
