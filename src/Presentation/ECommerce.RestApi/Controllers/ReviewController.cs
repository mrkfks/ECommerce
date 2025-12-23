using ECommerce.Application.DTOs;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.RestApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "SameCompanyOrSuperAdmin")]
public class ReviewController : ControllerBase
{
    private readonly AppDbContext _context;

    public ReviewController(AppDbContext context)
    {
        _context = context;
    }
    
    [HttpPost]
    [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
    public async Task<IActionResult> Add(ReviewCreateDto dto)
    {
        var review = Review.Create(
            dto.ProductId,
            dto.CustomerId,
            dto.CompanyId,
            dto.ReviewerName ?? "Anonim",
            dto.Rating,
            dto.Comment
        );
        
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();
        
        return Ok(new { id = review.Id, message = "Yorum eklendi" });
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var review = await _context.Reviews
            .Include(r => r.Product)
            .Include(r => r.Customer)
            .AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new ReviewDto
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
            })
            .FirstOrDefaultAsync();
            
        if (review == null)
            return NotFound(new { message = "Yorum Bulunamadı" });
        return Ok(review);
    }
    
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var reviews = await _context.Reviews
            .Include(r => r.Product)
            .Include(r => r.Customer)
            .AsNoTracking()
            .Select(r => new ReviewDto
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
            })
            .ToListAsync();
        return Ok(reviews);
    }

    [HttpGet("product/{productId}")]
    public async Task<IActionResult> GEtByProduct(int productId)
    {
        var reviews = await _context.Reviews
            .Include(r => r.Product)
            .Include(r => r.Customer)
            .Where(r => r.ProductId == productId)
            .AsNoTracking()
            .Select(r => new ReviewDto
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
            })
            .ToListAsync();
        return Ok(reviews);
    }
    
    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetByCustomer(int customerId)
    {
        var reviews = await _context.Reviews
            .Include(r => r.Product)
            .Include(r => r.Customer)
            .Where(r => r.CustomerId == customerId)
            .AsNoTracking()
            .Select(r => new ReviewDto
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
            })
            .ToListAsync();
        return Ok(reviews);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
    public async Task<IActionResult> Update(int id, ReviewUpdateDto dto)
    {
        if (id != dto.Id) 
            return BadRequest();
            
        var review = await _context.Reviews.FindAsync(id);
        if (review == null)
            return NotFound(new { message = "Yorum bulunamadı" });
            
        review.Update(dto.Rating, dto.Comment);
        await _context.SaveChangesAsync();
        
        return Ok(new { message = "Yorum güncellendi." });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
    public async Task<IActionResult> Delete(int id)
    {
        var review = await _context.Reviews.FindAsync(id);
        if (review == null)
            return NotFound(new { message = "Yorum bulunamadı" });
            
        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();
        
        return Ok(new { message = "Yorum başarıyla silindi." });
    }
}