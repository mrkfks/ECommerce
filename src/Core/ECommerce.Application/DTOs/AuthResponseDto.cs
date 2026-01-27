namespace ECommerce.Application.DTOs;

public record AuthResponseDto(
    string Token,
    string RefreshToken,
    DateTime Expiration,
    UserDto User
);
