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

    public async Task<ReviewDto> AddAsync(ReviewCreateDto dto)
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

    public async Task UpdateAsync(int id, ReviewUpdateDto dto)
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
}
