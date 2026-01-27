namespace ECommerce.Application.DTOs;

public record RefreshTokenRequest(
    string Token,
    string RefreshToken
);
