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

    // SuperAdmin: Talep detayını görebilsin
    [HttpGet("{id}")]
    [Authorize(Roles = "SuperAdmin")]
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
            var request = new ECommerce.Domain.Entities.Request
            {
                CompanyId = dto.CompanyId,
                Title = dto.Title,
                Description = dto.Description,
                Feedback = null,
                Status = ECommerce.Domain.Entities.RequestStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

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

    // SuperAdmin: Talep durumunu güncellesin
    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> UpdateRequest(int id, [FromBody] RequestUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var request = await _unitOfWork.Requests.GetByIdAsync(id);
            if (request == null)
                return NotFound(new { error = "Talep bulunamadı" });

            request.Title = dto.Title;
            request.Description = dto.Description;
            request.Status = (ECommerce.Domain.Entities.RequestStatus)dto.Status;
            request.Feedback = string.IsNullOrWhiteSpace(dto.Feedback) ? null : dto.Feedback.Trim();
            request.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Requests.Update(request);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { message = "Talep güncellendi", status = (int)request.Status });
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

            request.Status = ECommerce.Domain.Entities.RequestStatus.Approved;
            request.Feedback = string.IsNullOrWhiteSpace(dto?.Feedback) ? request.Feedback : dto.Feedback!.Trim();
            request.UpdatedAt = DateTime.UtcNow;

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

            request.Status = ECommerce.Domain.Entities.RequestStatus.Rejected;
            request.Feedback = string.IsNullOrWhiteSpace(dto?.Feedback) ? request.Feedback : dto.Feedback!.Trim();
            request.UpdatedAt = DateTime.UtcNow;

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
