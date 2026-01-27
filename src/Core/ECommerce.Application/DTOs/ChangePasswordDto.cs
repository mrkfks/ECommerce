namespace ECommerce.Application.DTOs;

public record ChangePasswordDto(
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword
);
