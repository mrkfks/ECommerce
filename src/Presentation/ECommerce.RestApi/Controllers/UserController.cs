using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.RestApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // CREATE
        [HttpPost]
        public async Task<IActionResult> Add(UserCreateDto dto)
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
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        // READ - Get all by Company
        [HttpGet("company/{companyId}")]
        public async Task<IActionResult> GetByCompany(int companyId)
        {
            var users = await _userService.GetByCompanyAsync(companyId);
            return Ok(users);
        }

        // UPDATE
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UserUpdateDto dto)
        {
             if(id != dto.Id) return BadRequest();
             await _userService.UpdateAsync(dto);
             return Ok(new { message = "Kullanıcı güncellendi." });
        }

        // DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _userService.DeleteAsync(id);
            return Ok(new { message = "Kullanıcı başarıyla silindi." });
        }

        // ROLE ASSIGNMENT
        [HttpPost("{id}/assign-role")]
        public async Task<IActionResult> AssignRole(int id, [FromBody] string roleName)
        {
            await _userService.AddRoleAsync(id, roleName);
            return Ok(new { message = "Rol atama işlemi tamamlandı." });
        }
}