namespace ECommerce.Application.DTOs;

public record CompanyFormDto(
    int? Id,
    string Name,
    string? Description = null,
    string TaxNumber = "",
    string Email = "",
    string PhoneNumber = "",
    string Address = "",
    bool IsActive = true
);
