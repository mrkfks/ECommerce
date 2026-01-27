namespace ECommerce.Application.DTOs;

public record LoginDto(
    string LoginIdentifier, // Email or Username
    string Password
);
