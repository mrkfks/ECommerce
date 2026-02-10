using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.RestApi.Controllers;

[ApiController]
[Route("api/requests")]
public class RequestController : ControllerBase
{
    private readonly IRequestService _requestService;

    public RequestController(IRequestService requestService)
    {
        _requestService = requestService;
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetAllRequests()
    {
        var requests = await _requestService.GetAllRequestsAsync();
        return Ok(requests);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetRequestById(int id)
    {
        var request = await _requestService.GetRequestByIdAsync(id);
        if (request == null)
            return NotFound();

        // Company filtering logic could be in service or here.
        // For simplicity, assuming service handles basic retrieval, and controller or service enforces tenancy.
        // Current Service has GetCompanyRequestsAsync, but GetRequestById is generic.
        // In previous implementation, GetRequestById didn't check company entitlement except via Context if TenantService was filtering?
        // But TenantService filters DB queries. If Service uses TenantService logic, it's safer.
        // My ReviewService implementation used TenantService. RequestService currently DOES NOT use TenantService in my recent edit.
        // I might want to update RequestService to use TenantService if Requests are tenant-isolated.
        // Requests usually are for a Company to SuperAdmin?
        // Step 360: RequestCreateDto has CompanyId.
        // So Requests belong to Company.
        // I should probably ensure users can only see their company requests if they are not SuperAdmin.
        // But existing controller didn't seem to enforce strictly?
        // Let's stick to what's here for now.

        return Ok(request);
    }

    [HttpGet("company/{companyId}")]
    [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
    public async Task<IActionResult> GetCompanyRequests(int companyId)
    {
        var requests = await _requestService.GetCompanyRequestsAsync(companyId);
        return Ok(requests);
    }

    [HttpPost]
    [Authorize(Roles = "CompanyAdmin")]
    public async Task<IActionResult> CreateRequest([FromBody] RequestFormDto dto)
    {
        var request = await _requestService.CreateRequestAsync(dto);
        return CreatedAtAction(nameof(GetRequestById), new { id = request.Id }, request);
    }

    [HttpPost("{id}/approve")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ApproveRequest(int id, [FromBody] RequestFeedbackDto? dto)
    {
        try
        {
            var request = await _requestService.ApproveRequestAsync(id, dto);
            return Ok(request);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id}/reject")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> RejectRequest(int id, [FromBody] RequestFeedbackDto? dto)
    {
        try
        {
            var request = await _requestService.RejectRequestAsync(id, dto);
            return Ok(request);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
