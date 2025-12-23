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
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == loginDto.UsernameOrEmail || u.Username == loginDto.UsernameOrEmail);

            if (user == null)
            {
                throw new UnauthorizedException("Invalid credentials");
            }

            var passwordOk = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash);
            if (!passwordOk)
            {
                var legacy = LegacyHashPassword(loginDto.Password);
                if (legacy == user.PasswordHash)
                {
                    user.UpdatePassword(BCrypt.Net.BCrypt.HashPassword(loginDto.Password));
                    await _context.SaveChangesAsync();
                    passwordOk = true;
                }
            }

            if (!passwordOk)
            {
                throw new UnauthorizedException("Invalid credentials");
            }

            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == user.CompanyId);
            if (company == null || !company.IsApproved)
            {
                throw new ForbiddenException("Company is not approved");
            }

            var token = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            return new AuthResponseDto
            {
                AccessToken = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                Username = user.Username,
                Roles = user.UserRoles.Select(ur => ur.Role?.Name ?? "").ToList()
            };
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                throw new ConflictException("Email already exists");
            }

            var user = User.Create(
                registerDto.CompanyId,
                registerDto.Username,
                registerDto.Email,
                BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                registerDto.FirstName,
                registerDto.LastName
            );

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            return new AuthResponseDto
            {
                AccessToken = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                Username = user.Username,
                Roles = new List<string>()
            };
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

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? "SecretKeySecretKey12345678");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("userId", user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("CompanyId", user.CompanyId.ToString())
            };

            foreach (var userRole in user.UserRoles)
            {
                if (userRole.Role != null)
                    claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Name));
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
    }
}
