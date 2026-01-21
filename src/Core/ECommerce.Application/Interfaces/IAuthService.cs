using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);

        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);

        Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);

        Task LogoutAsync(int userId);

        Task<bool> ValidateTokenAsync(string token);

        Task<UserDto?> GetUserByIdAsync(int userId);

        Task<bool> IsEmailAvailableAsync(string email);

        Task<bool> IsUsernameAvailableAsync(string username);

        Task<UserDto> UpdateProfileAsync(int userId, UserProfileUpdateDto dto);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto dto);
    }
}

