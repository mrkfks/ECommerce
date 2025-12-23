using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ECommerce.Application.DTOs;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ECommerce.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            var userData = await _context.Users
                .IgnoreQueryFilters() // Bypass tenant filter for authentication
                .Where(u => u.Email == loginDto.UsernameOrEmail || u.Username == loginDto.UsernameOrEmail)
                .Select(u => new
                {
                    u.Id,
                    u.CompanyId,
                    u.Username,
                    u.Email,
                    u.PasswordHash,
                    Roles = u.UserRoles.Select(ur => ur.Role != null ? ur.Role.Name : "").ToList()
                })
                .FirstOrDefaultAsync();

            if (userData == null)
            {
                throw new UnauthorizedException("Invalid credentials");
            }

            var passwordOk = BCrypt.Net.BCrypt.Verify(loginDto.Password, userData.PasswordHash);
            if (!passwordOk)
            {
                // Legacy SHA256 kontrolü
                var legacy = LegacyHashPassword(loginDto.Password);
                if (legacy == userData.PasswordHash)
                {
                    var userToUpdate = await _context.Users.FirstOrDefaultAsync(u => u.Id == userData.Id);
                    if (userToUpdate != null)
                    {
                        userToUpdate.UpdatePassword(BCrypt.Net.BCrypt.HashPassword(loginDto.Password));
                        await _context.SaveChangesAsync();
                        passwordOk = true;
                    }
                }
            }

            if (!passwordOk)
            {
                throw new UnauthorizedException("Invalid credentials");
            }

            // Şirket onay kontrolü - CompanyAdmin veya CompanyUser için
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == userData.CompanyId);
            if (company != null && !company.IsApproved)
            {
                throw new ForbiddenException("Şirketiniz henüz onaylanmamıştır. Lütfen süper admin onayını bekleyiniz.");
            }

            var token = GenerateJwtToken(userData.Id, userData.Username, userData.Email, userData.CompanyId, userData.Roles);
            var refreshToken = GenerateRefreshToken();

            return new AuthResponseDto
            {
                AccessToken = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                Username = userData.Username,
                Roles = userData.Roles
            };
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                throw new ConflictException("Email already exists");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // CompanyId zorunlu - sadece mevcut şirketlere kullanıcı ekleyebilir
                if (registerDto.CompanyId <= 0)
                {
                    throw new ArgumentException("Şirket ID'si gereklidir. Yeni şirket kaydı için /api/Company/register endpoint'ini kullanın.");
                }

                // Şirketin mevcut ve onaylı olduğunu kontrol et
                var company = await _context.Companies
                    .FirstOrDefaultAsync(c => c.Id == registerDto.CompanyId);
                
                if (company == null)
                {
                    throw new NotFoundException("Belirtilen şirket bulunamadı.");
                }

                if (!company.IsApproved)
                {
                    throw new ForbiddenException("Bu şirket henüz onaylanmamış. Kullanıcı eklenemez.");
                }

                int companyId = registerDto.CompanyId;
                bool isNewCompany = false;

                var user = User.Create(
                    companyId,
                    registerDto.Username,
                    registerDto.Email,
                    BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                    registerDto.FirstName,
                    registerDto.LastName
                );

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Assign Role - sadece User rolü (çünkü mevcut şirkete ekleniyor)
                var roleName = "User";
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
                var roles = new List<string>();

                if (role != null)
                {
                    var userRole = UserRole.Create(user.Id, role.Id, role.Name);
                    _context.UserRoles.Add(userRole);
                    await _context.SaveChangesAsync();
                    roles.Add(roleName);
                }

                await transaction.CommitAsync();

                // Onaylı şirkete kullanıcı eklendi, token ver
                var token = GenerateJwtToken(user.Id, user.Username, user.Email, user.CompanyId, roles);
                var refreshToken = GenerateRefreshToken();

                return new AuthResponseDto
                {
                    AccessToken = token,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                    Username = user.Username,
                    Roles = roles
                };
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
        {
            // Token'ı validate et - bu basit implementasyon refresh token'ı token içinde saklamaz
            // Gerçek uygulamada, refresh token'ları DB'de saklamalısınız
            // Şimdilik JWT token süresi dolmuşsa yeni bir token oluştur
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? "SecretKeySecretKey12345678");

            try
            {
                // Refresh token'ı validate et
                var principal = tokenHandler.ValidateToken(refreshToken, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = false, // Süresi dolmuş olabilir
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    throw new UnauthorizedException("Invalid token");
                }

                var user = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null || !user.IsActive)
                {
                    throw new UnauthorizedException("User not found or inactive");
                }

                var newAccessToken = GenerateJwtToken(user);
                var newRefreshToken = GenerateRefreshToken();

                return new AuthResponseDto
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                    Username = user.Username,
                    Roles = user.UserRoles.Select(ur => ur.Role?.Name ?? "").ToList()
                };
            }
            catch (AppException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new UnauthorizedException("Invalid refresh token: " + ex.Message);
            }
        }

        public async Task LogoutAsync(int userId)
        {
            // Refresh token revocation can be implemented here
            await Task.CompletedTask;
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            return !string.IsNullOrEmpty(token);
        }

        private string GenerateJwtToken(int userId, string username, string email, int companyId, IEnumerable<string> roles)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? "SecretKeySecretKey12345678");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim("userId", userId.ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Email, email),
                new Claim("CompanyId", companyId.ToString())
            };

            foreach (var roleName in roles)
            {
                if (!string.IsNullOrWhiteSpace(roleName))
                {
                    claims.Add(new Claim(ClaimTypes.Role, roleName));
                }
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(60),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateJwtToken(User user)
        {
            var roles = user.UserRoles.Select(ur => ur.Role?.Name ?? "");
            return GenerateJwtToken(user.Id, user.Username, user.Email, user.CompanyId, roles);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        private string LegacyHashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        public async Task<UserDto?> GetUserByIdAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return null;

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                CompanyId = user.CompanyId,
                CompanyName = user.Company?.Name,
                Roles = user.UserRoles.Select(ur => ur.Role?.Name ?? "").ToList(),
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }
    }
}
