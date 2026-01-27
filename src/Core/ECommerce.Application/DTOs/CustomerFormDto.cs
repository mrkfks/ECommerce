namespace ECommerce.Application.DTOs;

public record CustomerFormDto(
    int? Id,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    DateTime DateOfBirth,
    int CompanyId,
    int? UserId = null
);
