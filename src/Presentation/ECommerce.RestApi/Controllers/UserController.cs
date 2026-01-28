using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.RestApi.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Policy = "SameCompanyOrSuperAdmin")]
public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // CREATE
        [HttpPost]
        [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
        public async Task<IActionResult> Add(UserFormDto dto)
        {
            var user = await _userService.CreateAsync(dto);
            return Ok(user);
        }

        // READ - Get by Id
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "Kullanıcı bulunamadı." });

            return Ok(user);
        }

        // READ - Get All
        [HttpGet]
        [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
        public async Task<IActionResult> GetAll([FromQuery] int? companyId = null)
        {
            if (companyId.HasValue)
            {
                var users = await _userService.GetByCompanyAsync(companyId.Value);
                return Ok(users);
            }
            else
            {
                var users = await _userService.GetAllAsync();
                return Ok(users);
            }
        }

        // READ - Get all by Company
        [HttpGet("company/{companyId}")]
        [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
        public async Task<IActionResult> GetByCompany(int companyId)
        {
            var users = await _userService.GetByCompanyAsync(companyId);
            return Ok(users);
        }

        // UPDATE
        [HttpPut("{id}")]
        [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
        public async Task<IActionResult> Update(int id, UserFormDto dto)
        {
             if(id != dto.Id) return BadRequest();
             
             try
             {
                 await _userService.UpdateAsync(dto);
                 
                 // Rolleri güncelle
                 if (dto.Roles != null && dto.Roles.Any())
                 {
                     // Önce mevcut rolleri al
                     var currentRoles = await _userService.GetRolesAsync(id);
                     
                     // Silinmesi gereken roller
                     foreach (var role in currentRoles)
                     {
                         if (!dto.Roles.Contains(role))
                         {
                             await _userService.RemoveRoleAsync(id, role);
                         }
                     }
                     
                     // Eklenmesi gereken roller
                     foreach (var role in dto.Roles)
                     {
                         if (!currentRoles.Contains(role))
                         {
                             await _userService.AddRoleAsync(id, role);
                         }
                     }
                 }
                 
                 // Aktif durumu güncelle
                 await _userService.SetActiveStatusAsync(id, dto.IsActive);
                 
                 return Ok(new { message = "Kullanıcı güncellendi." });
             }
             catch (Exception ex)
             {
                 return BadRequest(new { message = ex.Message });
             }
        }

        // DELETE
        [HttpDelete("{id}")]
        [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _userService.DeleteAsync(id);
            return Ok(new { message = "Kullanıcı başarıyla silindi." });
        }

        // ROLE ASSIGNMENT
        [HttpPost("{id}/assign-role")]
        [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
        public async Task<IActionResult> AssignRole(int id, [FromBody] RoleAssignmentDto dto)
        {
            await _userService.AddRoleAsync(id, dto.RoleName);
            return Ok(new { message = "Rol atama işlemi tamamlandı." });
        }

        // UPDATE PROFILE - Herkes kendi profilini düzenleyebilir
        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UserProfileUpdateDto dto)
        {
            // Kullanıcının kendi ID'sini token'dan al
            var userIdClaim = User.FindFirst("userId") ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Kullanıcı kimliği bulunamadı." });
            }

            try
            {
                // Kullanıcının kendi profilini güncellemesine izin ver
                var updateDto = new UserFormDto
                {
                    Id = userId,
                    Username = dto.Username ?? string.Empty,
                    Email = dto.Email ?? string.Empty,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    IsActive = true, // Kullanıcı kendi durumunu değiştiremez
                    Roles = new List<string>() // Kullanıcı kendi rollerini değiştiremez
                };

                await _userService.UpdateAsync(updateDto);
                return Ok(new { message = "Profil bilgileriniz güncellendi." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET PROFILE - Kendi profilini görüntüle
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userIdClaim = User.FindFirst("userId") ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Kullanıcı kimliği bulunamadı." });
            }

            var user = await _userService.GetByIdAsync(userId);
            if (user == null)
                return NotFound(new { message = "Kullanıcı bulunamadı." });

            return Ok(user);
        }
}