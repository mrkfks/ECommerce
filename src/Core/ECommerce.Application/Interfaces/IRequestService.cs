using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces;

public interface IRequestService
{
    Task<IReadOnlyList<RequestDto>> GetAllRequestsAsync();
    Task<RequestDto?> GetRequestByIdAsync(int id);
    Task<IReadOnlyList<RequestDto>> GetCompanyRequestsAsync(int companyId);
    Task<RequestDto> CreateRequestAsync(RequestCreateDto dto);
    Task<RequestDto> ApproveRequestAsync(int id, RequestFeedbackDto? dto);
    Task<RequestDto> RejectRequestAsync(int id, RequestFeedbackDto? dto);
}
