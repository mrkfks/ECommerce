namespace ECommerce.Application.DTOs;

public record ReviewFormDto(
    int? Id,
    int? ProductId = null,
    int? CustomerId = null,
    int? CompanyId = null,
    string? ReviewerName = null,
    int Rating = 0,
    string Comment = ""
);
