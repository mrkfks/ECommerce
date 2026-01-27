namespace ECommerce.Application.DTOs;

public record RegisterDto(
    string FirstName,
    string LastName,
    string Username,
    string Email,
    string Password,
    string ConfirmPassword,
    string PhoneNumber,
    int CompanyId,
    string CompanyName = "",
    string CompanyAddress = "",
    string CompanyPhoneNumber = "",
    string CompanyEmail = "",
    string TaxNumber = ""
);
