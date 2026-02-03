using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.RestApi.Controllers;

[ApiController]
[Route("api/wishlist")]
public class WishlistController : ControllerBase
{
    private readonly IWishlistService _wishlistService;
    private readonly ILogger<WishlistController> _logger;

    public WishlistController(IWishlistService wishlistService, ILogger<WishlistController> logger)
    {
        _wishlistService = wishlistService;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetWishlist([FromQuery] string? sessionId)
    {
        try
        {
            _logger.LogInformation("GetWishlist called with sessionId: {SessionId}", sessionId);
            var wishlist = await _wishlistService.GetWishlistAsync(sessionId);
            return Ok(new { success = true, data = wishlist });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting wishlist: {Message}", ex.Message);
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("items")]
    [AllowAnonymous]
    public async Task<IActionResult> AddToWishlist([FromBody] AddToWishlistDto dto, [FromQuery] string? sessionId)
    {
        try
        {
            await _wishlistService.AddToWishlistAsync(dto.ProductId, sessionId);
            return Ok(new { success = true, message = "Item added to wishlist" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding to wishlist");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpDelete("items/{itemId}")]
    [AllowAnonymous]
    public async Task<IActionResult> RemoveFromWishlist(int itemId)
    {
        try
        {
            await _wishlistService.RemoveFromWishlistAsync(itemId);
            return Ok(new { success = true, message = "Item removed from wishlist" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing from wishlist");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpDelete]
    [AllowAnonymous]
    public async Task<IActionResult> ClearWishlist([FromQuery] string? sessionId)
    {
        try
        {
            await _wishlistService.ClearWishlistAsync(sessionId);
            return Ok(new { success = true, message = "Wishlist cleared" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing wishlist");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}
