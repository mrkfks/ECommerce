using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.RestApi.Controllers;

[ApiController]
[Route("api/reviews")]
[Authorize(Policy = "SameCompanyOrSuperAdmin")]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }
    
    [HttpPost]
    [Authorize(Roles = "CompanyAdmin,User,SuperAdmin")]
    public async Task<IActionResult> Add(ReviewFormDto dto)
    {
        var review = await _reviewService.AddAsync(dto);
        return Ok(new { id = review.Id, message = "Yorum eklendi" });
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var review = await _reviewService.GetByIdAsync(id);
        if (review == null)
            return NotFound(new { message = "Yorum Bulunamadı" });
        return Ok(review);
    }
    
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var reviews = await _reviewService.GetAllAsync();
        return Ok(reviews);
    }

    [HttpGet("product/{productId}")]
    public async Task<IActionResult> GetByProduct(int productId)
    {
        var reviews = await _reviewService.GetByProductIdAsync(productId);
        return Ok(reviews);
    }
    
    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetByCustomer(int customerId)
    {
        var reviews = await _reviewService.GetByCustomerIdAsync(customerId);
        return Ok(reviews);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "CompanyAdmin,User,SuperAdmin")]
    public async Task<IActionResult> Update(int id, ReviewFormDto dto)
    {
        if (id != dto.Id) 
            return BadRequest();
            
        try
        {
            await _reviewService.UpdateAsync(id, dto);
            return Ok(new { message = "Yorum güncellendi." });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Yorum bulunamadı" });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "CompanyAdmin,User,SuperAdmin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _reviewService.DeleteAsync(id);
            return Ok(new { message = "Yorum başarıyla silindi." });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Yorum bulunamadı" });
        }
    }
}