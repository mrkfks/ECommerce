using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces
{
    public interface IUserService
    {
        // Tek kullanıcı bilgisi getir
        Task<UserDto?> GetByIdAsync(int id);

        // Tüm kullanıcıları getir
        Task<IReadOnlyList<UserDto>> GetAllAsync();

        // Kullanıcı adı ile kullanıcı getir
        Task<UserDto?> GetByUsernameAsync(string username);

        // Yeni kullanıcı oluştur
        Task<UserDto> CreateAsync(UserCreateDto dto);

        // Mevcut kullanıcı güncelle
        Task UpdateAsync(UserUpdateDto dto);

        // Kullanıcı sil
        Task DeleteAsync(int id);

        // Kullanıcının rollerini getir
        Task<IReadOnlyList<string>> GetRolesAsync(int userId);

        // Kullanıcıya rol ekle
        Task AddRoleAsync(int userId, string role);

        // Kullanıcıdan rol kaldır
        Task RemoveRoleAsync(int userId, string role);

        // Kullanıcı aktif/pasif durumu güncelle
        Task SetActiveStatusAsync(int userId, bool isActive);
    }
}
