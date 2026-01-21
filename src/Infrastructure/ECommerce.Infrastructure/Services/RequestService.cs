using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services;

public class RequestService : IRequestService
{
    private readonly AppDbContext _context;
    private readonly ILogger<RequestService> _logger;

    public RequestService(AppDbContext context, ILogger<RequestService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<RequestDto>> GetAllRequestsAsync()
    {
        var requests = await _context.Requests.AsNoTracking().ToListAsync();
        return requests.Select(MapToDto).ToList();
    }

    public async Task<RequestDto?> GetRequestByIdAsync(int id)
    {
        var request = await _context.Requests.FindAsync(id);
        return request == null ? null : MapToDto(request);
    }

    public async Task<IReadOnlyList<RequestDto>> GetCompanyRequestsAsync(int companyId)
    {
        var requests = await _context.Requests.Where(r => r.CompanyId == companyId).AsNoTracking().ToListAsync();
        return requests.Select(MapToDto).ToList();
    }

    public async Task<RequestDto> CreateRequestAsync(RequestCreateDto dto)
    {
        var request = Request.Create(
            dto.CompanyId,
            dto.Title,
            dto.Description
        );

        _context.Requests.Add(request);
        await _context.SaveChangesAsync();
        
        return MapToDto(request);
    }

    public async Task<RequestDto> ApproveRequestAsync(int id, RequestFeedbackDto? dto)
    {
        var request = await _context.Requests.FindAsync(id);
        if (request == null) throw new KeyNotFoundException("Request not found");
        
        request.Approve(string.IsNullOrWhiteSpace(dto?.Feedback) ? null : dto.Feedback!.Trim());
        await _context.SaveChangesAsync();
        
        return MapToDto(request);
    }

    public async Task<RequestDto> RejectRequestAsync(int id, RequestFeedbackDto? dto)
    {
        var request = await _context.Requests.FindAsync(id);
        if (request == null) throw new KeyNotFoundException("Request not found");
        
        request.Reject(string.IsNullOrWhiteSpace(dto?.Feedback) ? null : dto.Feedback!.Trim());
        await _context.SaveChangesAsync();
        
        return MapToDto(request);
    }
    
    // Explicit full path for Request entity if conflict occurs, but here we see if using works.
    // Assuming Request is Domain Entity. If ambiguity with RequestController, use full name.
    // Here we are in Service, so Request should be Entity.
    
    private static RequestDto MapToDto(ECommerce.Domain.Entities.Request r)
    {
        return new RequestDto
        {
            Id = r.Id,
            CompanyId = r.CompanyId,
            Title = r.Title,
            Description = r.Description,
            Feedback = r.Feedback,
            Status = (int)r.Status,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        };
    }
}
