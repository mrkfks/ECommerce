using System.Security.Claims;
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
    private readonly ICustomerService _customerService;

    public ReviewController(IReviewService reviewService, ICustomerService customerService)
    {
        _reviewService = reviewService;
        _customerService = customerService;
    }

    [HttpPost]
    [AllowAnonymous] // Giriş yapmış kullanıcılar yorum yapabilir - yetkilendirme metod içinde yapılıyor
    public async Task<IActionResult> Add([FromBody] ReviewFormDto dto)
    {
        Console.WriteLine($"[ReviewController.Add] Called with ProductId={dto.ProductId}, Rating={dto.Rating}");
        try
        {
            // Token'dan userId al
            var userIdClaim = User.FindFirst("userId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine($"[ReviewController.Add] UserIdClaim={userIdClaim}");

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                Console.WriteLine("[ReviewController.Add] Unauthorized - no userId");
                return Unauthorized(new { message = "Yorum yapabilmek için giriş yapmalısınız" });
            }

            // CompanyId'yi token'dan al
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            int.TryParse(companyIdClaim, out var companyId);
            Console.WriteLine($"[ReviewController.Add] UserId={userId}, CompanyId={companyId}");

            // UserId'den CustomerId bul veya oluştur
            var customer = await _customerService.GetByUserIdAsync(userId);
            var customerId = customer?.Id ?? 0;
            Console.WriteLine($"[ReviewController.Add] CustomerId={customerId}");

            // DTO'yu güncelle
            var reviewDto = dto with
            {
                CustomerId = customerId > 0 ? customerId : dto.CustomerId,
                CompanyId = companyId > 0 ? companyId : dto.CompanyId,
                ReviewerName = dto.ReviewerName ?? User.Identity?.Name ?? "Anonim"
            };

            var review = await _reviewService.AddAsync(reviewDto);
            Console.WriteLine($"[ReviewController.Add] Review created with Id={review.Id}");
            return Ok(new { id = review.Id, message = "Yorum eklendi" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ReviewController.Add] Error: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
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
    [AllowAnonymous]
    public async Task<IActionResult> GetByProduct(int productId)
    {
        var reviews = await _reviewService.GetByProductIdAsync(productId);
        return Ok(reviews);
    }

    /// <summary>
    /// Ürün değerlendirme özeti
    /// </summary>
    [HttpGet("product/{productId}/summary")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProductSummary(int productId)
    {
        var summary = await _reviewService.GetProductSummaryAsync(productId);
        return Ok(summary);
    }

    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetByCustomer(int customerId)
    {
        var reviews = await _reviewService.GetByCustomerIdAsync(customerId);
        return Ok(reviews);
    }

    /// <summary>
    /// Kullanıcının kendi yorumları
    /// </summary>
    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMyReviews()
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized();
        }

        var reviews = await _reviewService.GetMyReviewsAsync(userId);
        return Ok(reviews);
    }

    /// <summary>
    /// Kullanıcı bu ürüne yorum yapabilir mi?
    /// </summary>
    [HttpGet("can-review/{productId}")]
    [Authorize]
    public async Task<IActionResult> CanReview(int productId)
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized();
        }

        var result = await _reviewService.CanReviewAsync(productId, userId);
        return Ok(result);
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