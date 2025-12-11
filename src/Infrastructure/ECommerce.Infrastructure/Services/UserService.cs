using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace ECommerce.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserDto?> GetByIdAsync(int id)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
                
            return user == null ? null : MapToDto(user);
        }

        public async Task<IReadOnlyList<UserDto>> GetAllAsync()
        {
            var users = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ToListAsync();
            return users.Select(MapToDto).ToList();
        }

        public async Task<IReadOnlyList<UserDto>> GetByCompanyAsync(int companyId)
        {
            var users = await _context.Users
                .Where(u => u.CompanyId == companyId)
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ToListAsync();
            return users.Select(MapToDto).ToList();
        }

        public async Task<UserDto?> GetByUsernameAsync(string username)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == username);
            return user == null ? null : MapToDto(user);
        }

        public async Task<UserDto> CreateAsync(UserCreateDto dto)
        {
            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                // Assuming simple hashing here, consistent with AuthService
                PasswordHash = HashPassword(dto.Password), 
                CompanyId = dto.CompanyId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UserRoles = new List<UserRole>()
            };

            // Handle Role assignment if DTO has it, or default
            
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return MapToDto(user);
        }

        // Controller uses: AddAsync(username, password, companyId)
        // IUserService has: CreateAsync(UserCreateDto).
        // I will stick to Interface usage in this file. I will fix Controller later.
        // However, to make migration easier, I can add the controller-compatible method as an overload?
        // No, fixing Controller is better for long term.

        public async Task UpdateAsync(UserUpdateDto dto)
        {
            var user = await _context.Users.FindAsync(dto.Id);
            if(user != null)
            {
                user.Username = dto.Username;
                user.Email = dto.Email;
                user.CompanyId = dto.CompanyId;
                user.IsActive = dto.IsActive;
                await _context.SaveChangesAsync();
            }
        }
        
        // Helper for Controller compatibility until refactor
         public async Task<UserDto> AddAsync(string username, string password, int companyId)
         {
             var dto = new UserCreateDto { Username = username, Password = password, CompanyId = companyId, Email = username + "@example.com" }; // Placeholder email
             return await CreateAsync(dto);
         }
         
         public async Task<UserDto?> UpdateAsync(int id, string username, string password, int companyId)
         {
             var user = await _context.Users.FindAsync(id);
             if(user == null) return null;
             
             user.Username = username;
             user.CompanyId = companyId;
             if(!string.IsNullOrEmpty(password)) 
                user.PasswordHash = HashPassword(password);
                
             await _context.SaveChangesAsync();
             return MapToDto(user);
         }

        public async Task DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if(user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }
        
        // Controller compatibility
        public async Task<bool> DeleteAsync(int id, bool returnBool) 
        {
             var user = await _context.Users.FindAsync(id);
             if(user != null)
             {
                 _context.Users.Remove(user);
                 await _context.SaveChangesAsync();
                 return true;
             }
             return false;
        }

        public async Task<IReadOnlyList<string>> GetRolesAsync(int userId)
        {
            var roles = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.Role.Name)
                .ToListAsync();
            return roles;
        }

        public async Task AddRoleAsync(int userId, string roleName)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            if(role != null)
            {
                 if(!await _context.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == role.Id))
                 {
                     _context.UserRoles.Add(new UserRole { UserId = userId, RoleId = role.Id });
                     await _context.SaveChangesAsync();
                 }
            }
        }
        // Controller Compat
        public async Task<bool> AssignRoleAsync(int userId, string roleName)
        {
             try {
                 await AddRoleAsync(userId, roleName);
                 return true;
             } catch { return false; }
        }

        public async Task RemoveRoleAsync(int userId, string roleName)
        {
             var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
             if(role != null)
             {
                 var userRole = await _context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == role.Id);
                 if(userRole != null)
                 {
                     _context.UserRoles.Remove(userRole);
                     await _context.SaveChangesAsync();
                 }
             }
        }

        public async Task SetActiveStatusAsync(int userId, bool isActive)
        {
            var user = await _context.Users.FindAsync(userId);
            if(user != null)
            {
                user.IsActive = isActive;
                await _context.SaveChangesAsync();
            }
        }

        private static UserDto MapToDto(User u)
        {
            return new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                CompanyId = u.CompanyId,
                IsActive = u.IsActive,
                // Role handling in DTO might be needed
            };
        }
        
        private string HashPassword(string password)
        {
             using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }
    }
}
