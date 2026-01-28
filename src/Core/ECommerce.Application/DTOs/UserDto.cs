namespace ECommerce.Application.DTOs;

/// <summary>
/// Kullanıcı bilgisi DTO
/// </summary>
public record UserDto
{
    public int Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string? CustomerName { get; init; }  // Müşteri adı (eğer müşteri ise)
    public int? CompanyId { get; init; }
    public string? CompanyName { get; init; }
    public List<string>? Roles { get; init; }
    public bool IsActive { get; init; } = true;
    public DateTime? CreatedAt { get; init; }
}

/// <summary>
/// Kullanıcı oluşturma/güncelleme form DTO
/// </summary>
public record UserFormDto
{
    public int? Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? Password { get; init; }
    public int? CompanyId { get; init; }
    public List<string>? Roles { get; init; }
    public bool IsActive { get; init; } = true;
}

/// <summary>
/// Kullanıcı profili güncelleme DTO
/// </summary>
public record UserProfileUpdateDto
{
    public string? Username { get; init; }
    public string? Email { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
}

/// <summary>
/// Şifre değiştirme DTO
/// </summary>
public record ChangePasswordDto
{
    public string CurrentPassword { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
    public string ConfirmPassword { get; init; } = string.Empty;
}

/// <summary>
/// Rol bilgisi DTO
/// </summary>
public record RoleDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
