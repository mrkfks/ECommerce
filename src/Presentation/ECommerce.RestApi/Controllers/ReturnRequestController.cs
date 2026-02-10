using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.RestApi.Controllers;

[ApiController]
[Route("api/return-requests")]
[Authorize]
public class ReturnRequestController : ControllerBase
{
    private readonly IReturnRequestService _returnRequestService;

    public ReturnRequestController(IReturnRequestService returnRequestService)
    {
        _returnRequestService = returnRequestService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateReturnRequestDto dto)
    {
        try
        {
            // Model validation kontrolü
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                var errorMessage = string.Join("; ", errors.Select(e => e.ErrorMessage));
                return BadRequest(new { message = $"Validation hatası: {errorMessage}" });
            }

            var userId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Kullanıcı kimliği bulunamadı" });

            var result = await _returnRequestService.CreateAsync(dto, int.Parse(userId));
            return Ok(new
            {
                id = result.Id,
                message = "İade talebi başarıyla oluşturuldu."
            });
        }
        catch (Exception ex)
        {
            // Hata mesajını log'la
            Console.WriteLine($"[Create] Error: {ex.Message}");
            Console.WriteLine($"[Create] Stack: {ex.StackTrace}");

            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var returnRequest = await _returnRequestService.GetByIdAsync(id);
        if (returnRequest == null)
            return NotFound(new { message = "İade talebi bulunamadı" });

        return Ok(returnRequest);
    }

    [HttpGet("my-requests")]
    public async Task<IActionResult> GetMyRequests()
    {
        var userId = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Kullanıcı kimliği bulunamadı" });

        var requests = await _returnRequestService.GetMyReturnRequestsAsync(int.Parse(userId));
        return Ok(new ApiResponseDto<IReadOnlyList<ReturnRequestDto>>
        {
            Success = true,
            Data = requests,
            Message = "İade talepleri başarıyla getirildi"
        });
    }

    [HttpGet("order/{orderId}")]
    public async Task<IActionResult> GetByOrderId(int orderId)
    {
        var returnRequests = await _returnRequestService.GetByOrderIdAsync(orderId);
        return Ok(new ApiResponseDto<IReadOnlyList<ReturnRequestDto>>
        {
            Success = true,
            Data = returnRequests,
            Message = "Sipariş iade talepleri başarıyla getirildi"
        });
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        // Admin bypass header kontrolü (dashboard.web istekleri için)
        var adminBypass = Request.Headers.ContainsKey("X-Admin-Bypass");

        // Bypass header varsa izin ver
        if (adminBypass)
        {
            var returnRequests = await _returnRequestService.GetAllAsync();

            Console.WriteLine($"[API] GetAll() called. Returning {returnRequests.Count} requests");
            foreach (var req in returnRequests)
            {
                Console.WriteLine($"[API]   - {req.Id}: {req.CustomerName} | Order {req.OrderId} | {req.ProductName}");
            }

            return Ok(new ApiResponseDto<IReadOnlyList<ReturnRequestDto>>
            {
                Success = true,
                Data = returnRequests,
                Message = "İade talepleri başarıyla getirildi"
            });
        }

        return Unauthorized();
    }

    [HttpGet("pending")]
    [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
    public async Task<IActionResult> GetPending()
    {
        var returnRequests = await _returnRequestService.GetPendingRequestsAsync();
        return Ok(new ApiResponseDto<IReadOnlyList<ReturnRequestDto>>
        {
            Success = true,
            Data = returnRequests,
            Message = "Beklemede olan talepleri başarıyla getirildi"
        });
    }

    [HttpPut("{id}/status")]
    [AllowAnonymous]
    public async Task<IActionResult> UpdateStatus(int id, UpdateReturnRequestDto dto)
    {
        try
        {
            await _returnRequestService.UpdateStatusAsync(id, dto);
            return Ok(new ApiResponseDto<object>
            {
                Success = true,
                Data = null,
                Message = "İade talebi durumu başarıyla güncellendi"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponseDto<object>
            {
                Success = false,
                Data = null,
                Message = ex.Message
            });
        }
    }

    // TEST: Hızlı test veri eklemek için (development only - sil production'da!)
    [HttpPost("test/seed-data")]
    [AllowAnonymous]
    public async Task<IActionResult> SeedTestData()
    {
        try
        {
            var testRequest = new CreateReturnRequestDto
            {
                OrderId = 1,
                OrderItemId = 1,
                ProductId = 1,
                Quantity = 1,
                Reason = "Test: Ürün hasarlı",
                Comments = "Test veri"
            };

            var result = await _returnRequestService.CreateAsync(testRequest, 1);
            return Ok(new ApiResponseDto<ReturnRequestDto>
            {
                Success = true,
                Data = result,
                Message = "Test datası başarıyla oluşturuldu"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponseDto<object>
            {
                Success = false,
                Data = null,
                Message = $"Error: {ex.Message} | Inner: {ex.InnerException?.Message}"
            });
        }
    }
}
