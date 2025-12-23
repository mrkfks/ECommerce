using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.RestApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RequestController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public RequestController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // SuperAdmin: Tüm talepleri görebilsin
    [HttpGet]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetAllRequests()
    {
        try
        {
            var requests = await _unitOfWork.Requests.GetAllAsync();
            var dtos = requests.Select(r => new RequestDto
            {
                Id = r.Id,
                CompanyId = r.CompanyId,
                Title = r.Title,
                Description = r.Description,
                Feedback = r.Feedback,
                Status = (int)r.Status,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            }).ToList();
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Talep detayı: SuperAdmin tüm talepləri görebilir, kullanıcılar sadece kendi şirketlerinin taleplerini
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetRequestById(int id)
    {
        try
        {
            var request = await _unitOfWork.Requests.GetByIdAsync(id);
            if (request == null)
                return NotFound(new { error = "Talep bulunamadı" });

            var dto = new RequestDto
            {
                Id = request.Id,
                CompanyId = request.CompanyId,
                Title = request.Title,
                Description = request.Description,
                Feedback = request.Feedback,
                Status = (int)request.Status,
                CreatedAt = request.CreatedAt,
                UpdatedAt = request.UpdatedAt
            };
            return Ok(dto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Şirket: Kendi taleplerini görebilsin
    [HttpGet("company/{companyId}")]
    public async Task<IActionResult> GetCompanyRequests(int companyId)
    {
        try
        {
            var requests = await _unitOfWork.Requests.FindAsync(r => r.CompanyId == companyId);
            var dtos = requests.Select(r => new RequestDto
            {
                Id = r.Id,
                CompanyId = r.CompanyId,
                Title = r.Title,
                Description = r.Description,
                Feedback = r.Feedback,
                Status = (int)r.Status,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            }).ToList();
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Şirket: Talep gönderebilsin
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CreateRequest([FromBody] RequestCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var request = ECommerce.Domain.Entities.Request.Create(
                dto.CompanyId,
                dto.Title,
                dto.Description
            );

            await _unitOfWork.Requests.AddAsync(request);
            await _unitOfWork.SaveChangesAsync();

            var responseDto = new RequestDto
            {
                Id = request.Id,
                CompanyId = request.CompanyId,
                Title = request.Title,
                Description = request.Description,
                Feedback = request.Feedback,
                Status = (int)request.Status,
                CreatedAt = request.CreatedAt,
                UpdatedAt = request.UpdatedAt
            };

            return CreatedAtAction(nameof(GetRequestById), new { id = request.Id }, responseDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // SuperAdmin: Talep onaylasın
    [HttpPost("{id}/approve")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ApproveRequest(int id, [FromBody] RequestFeedbackDto? dto)
    {
        try
        {
            var request = await _unitOfWork.Requests.GetByIdAsync(id);
            if (request == null)
                return NotFound(new { error = "Talep bulunamadı" });

            request.Approve(string.IsNullOrWhiteSpace(dto?.Feedback) ? null : dto.Feedback!.Trim());

            _unitOfWork.Requests.Update(request);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { message = "Talep onaylandı", status = (int)request.Status });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // SuperAdmin: Talep reddetsin
    [HttpPost("{id}/reject")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> RejectRequest(int id, [FromBody] RequestFeedbackDto? dto)
    {
        try
        {
            var request = await _unitOfWork.Requests.GetByIdAsync(id);
            if (request == null)
                return NotFound(new { error = "Talep bulunamadı" });

            request.Reject(string.IsNullOrWhiteSpace(dto?.Feedback) ? null : dto.Feedback!.Trim());

            _unitOfWork.Requests.Update(request);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { message = "Talep reddedildi", status = (int)request.Status });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
