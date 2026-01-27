namespace ECommerce.Application.DTOs;

public record UserProfileUpdateDto(
    string FirstName,
    string LastName,
    string? Email = null,
    string? PhoneNumber = null,
    string? ProfilePictureUrl = null
);
