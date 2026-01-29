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
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace ECommerce.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(AppDbContext context, IConfiguration configuration, ILogger<AuthService> _logger)
        {
            _context = context;
            _configuration = configuration;
            this._logger = _logger;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            var identifier = !string.IsNullOrEmpty(loginDto.LoginIdentifier) 
                ? loginDto.LoginIdentifier 
                : loginDto.UsernameOrEmail;

            _logger.LogWarning($"[LOGIN] Giriş denemesi: {identifier}");
            
            var userData = await _context.Users
                .IgnoreQueryFilters()
                .Where(u => u.Email == identifier || u.Username == identifier)
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
                _logger.LogWarning($"[LOGIN] Kullanıcı bulunamadı: {loginDto.LoginIdentifier}");
                throw new UnauthorizedException("Invalid credentials");
            }
            _logger.LogWarning($"[LOGIN] Kullanıcı bulundu: {userData.Username} (id={userData.Id})");
            var passwordOk = BCrypt.Net.BCrypt.Verify(loginDto.Password, userData.PasswordHash);
            _logger.LogWarning($"[LOGIN] BCrypt kontrol sonucu: {passwordOk}");
            if (!passwordOk)
            {
                var legacy = LegacyHashPassword(loginDto.Password);
                _logger.LogWarning($"[LOGIN] Legacy hash kontrolü: {legacy == userData.PasswordHash}");
                if (legacy == userData.PasswordHash)
                {
                    var userToUpdate = await _context.Users.FirstOrDefaultAsync(u => u.Id == userData.Id);
                    if (userToUpdate != null)
                    {
                        userToUpdate.UpdatePassword(BCrypt.Net.BCrypt.HashPassword(loginDto.Password));
                        await _context.SaveChangesAsync();
                        passwordOk = true;
                        _logger.LogWarning($"[LOGIN] Legacy hash güncellendi ve şifre doğrulandı.");
                    }
                }
            }
            if (!passwordOk)
            {
                _logger.LogWarning($"[LOGIN] Şifre doğrulanamadı: {loginDto.LoginIdentifier}");
                throw new UnauthorizedException("Invalid credentials");
            }
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == userData.CompanyId);
            _logger.LogWarning($"[LOGIN] Şirket kontrolü: id={userData.CompanyId}, isApproved={(company != null ? company.IsApproved.ToString() : "null")}");
            if (company != null && !company.IsApproved)
            {
                _logger.LogWarning($"[LOGIN] Şirket onaysız: {company.Name}");
                throw new ForbiddenException("Şirketiniz henüz onaylanmamıştır. Lütfen süper admin onayını bekleyiniz.");
            }
            var token = GenerateJwtToken(userData.Id, userData.Username, userData.Email, userData.CompanyId, userData.Roles);
            var refreshToken = GenerateRefreshToken();
            _logger.LogWarning($"[LOGIN] Giriş başarılı: {userData.Username}");
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
            // Email ve Username kontrolü (TÜM ŞİRKETLERDE benzersiz olmalı)
            var existingUser = await _context.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Email == registerDto.Email || u.Username == registerDto.Username);

            if (existingUser != null)
            {
                if (existingUser.Email == registerDto.Email)
                    throw new ConflictException("Bu email adresi zaten kullanılıyor.");
                else
                    throw new ConflictException("Bu kullanıcı adı zaten kullanılıyor.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                int companyId;
                string roleName;

                // CompanyId 0 veya negatifse genel müşteri kaydı
                if (registerDto.CompanyId <= 0)
                {
                    // FK hatasını önlemek için varsayılan/ortak şirketi oluştur veya kullan
                    companyId = await EnsureDefaultCompanyIdAsync();
                    roleName = "Customer";
                }
                else
                {
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

                    companyId = registerDto.CompanyId;
                    roleName = "User";
                }

                var user = User.Create(
                    companyId,
                    registerDto.Username,
                    registerDto.Email,
                    BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                    registerDto.FirstName,
                    registerDto.LastName,
                    registerDto.PhoneNumber
                );

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Assign Role
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
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? "SecretKeySecretKey12345678");

            try
            {
                var principal = tokenHandler.ValidateToken(refreshToken, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)
                                ?? principal.Claims.FirstOrDefault(c => c.Type == "userId");

                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    throw new UnauthorizedException("Geçersiz refresh token");
                }

                var user = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null || !user.IsActive)
                {
                    throw new UnauthorizedException("Kullanıcı bulunamadı veya pasif");
                }

                var newAccessToken = GenerateJwtToken(user);
                var newRefreshToken = GenerateJwtToken(user); // Şimdilik basitleştirmek için JWT döndürüyoruz ki bir sonraki refresh çalışsın

                return new AuthResponseDto
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                    Username = user.Username,
                    Roles = user.UserRoles.Select(ur => ur.Role?.Name ?? "").ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RefreshToken hatası");
                throw new UnauthorizedException("Refresh token geçersiz veya süresi dolmuş.");
            }
        }

        public async Task LogoutAsync(int userId)
        {
            await Task.CompletedTask;
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            return await Task.FromResult(!string.IsNullOrEmpty(token));
        }

        private async Task<int> EnsureDefaultCompanyIdAsync()
        {
            // Varsayılan ya da mevcut ilk şirketi kullan; yoksa oluştur
            var company = await _context.Companies
                .IgnoreQueryFilters()
                .OrderBy(c => c.Id)
                .FirstOrDefaultAsync();

            if (company == null)
            {
                company = Company.Create(
                    "Default Company",
                    "N/A",
                    "0000000000",
                    "default@company.local",
                    "TAX-DEFAULT",
                    "System",
                    "0000000000",
                    "system@company.local");
                company.Approve();
                _context.Companies.Add(company);
                await _context.SaveChangesAsync();
            }
            else if (!company.IsApproved)
            {
                company.Approve();
                await _context.SaveChangesAsync();
            }

            return company.Id;
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

        public async Task<bool> IsEmailAvailableAsync(string email)
        {
            var existingUser = await _context.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Email == email);
            return existingUser == null;
        }

        public async Task<bool> IsUsernameAvailableAsync(string username)
        {
            var existingUser = await _context.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Username == username);
            return existingUser == null;
        }
        public async Task<UserDto> UpdateProfileAsync(int userId, UserProfileUpdateDto dto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("Kullanıcı bulunamadı.");
            }

            // Username/Email uniqueness check, only if changed
            if (user.Email != dto.Email)
            {
                var emailExists = await _context.Users.AnyAsync(u => u.Email == dto.Email && u.Id != userId);
                if (emailExists) throw new ConflictException("Bu email adresi zaten kullanılıyor.");
            }

            if (user.Username != dto.Username)
            {
                var usernameExists = await _context.Users.AnyAsync(u => u.Username == dto.Username && u.Id != userId);
                if (usernameExists) throw new ConflictException("Bu kullanıcı adı zaten kullanılıyor.");
            }

            user.UpdateProfile(dto.FirstName, dto.LastName, dto.Email ?? string.Empty, dto.Username);

            await _context.SaveChangesAsync();

            return await GetUserByIdAsync(userId) ?? throw new Exception("Updated user not found");
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("Kullanıcı bulunamadı.");
            }

            if (dto.NewPassword != dto.ConfirmPassword)
            {
                throw new BadRequestException("Yeni şifreler eşleşmiyor.");
            }

            var passwordOk = BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash);
            if (!passwordOk)
            {
                // Fallback for legacy
                var legacy = LegacyHashPassword(dto.CurrentPassword);
                if (legacy == user.PasswordHash)
                {
                    passwordOk = true;
                }
            }

            if (!passwordOk)
            {
                throw new BadRequestException("Mevcut şifre hatalı.");
            }

            user.UpdatePassword(BCrypt.Net.BCrypt.HashPassword(dto.NewPassword));
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
