namespace ECommerce.Application.DTOs;

public record RequestDto(
    int Id,
    int CompanyId,
    string Title,
    string Description,
    int Status,
    string? Feedback = null,
    DateTime? CreatedAt = null,
    DateTime? UpdatedAt = null
);

public record RequestFormDto(
    int? Id,
    int CompanyId,
    string Title,
    string Description,
    int Status = 0,
    string? Feedback = null
);
