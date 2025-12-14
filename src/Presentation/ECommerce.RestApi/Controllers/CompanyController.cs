using ECommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.RestApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompanyController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CompanyController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Policy = "RequireSuperAdmin")]
        public async Task<IActionResult> GetAll()
        {
            var companies = await _context.Companies.AsNoTracking().ToListAsync();
            return Ok(companies);
        }

        [HttpPost("{id:int}/approve")]
        [Authorize(Policy = "RequireSuperAdmin")]
        public async Task<IActionResult> Approve(int id)
        {
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == id);
            if (company == null) return NotFound();
            company.IsApproved = true;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Company approved" });
        }

        [HttpPost("{id:int}/deactivate")]
        [Authorize(Policy = "RequireSuperAdmin")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == id);
            if (company == null) return NotFound();
            company.IsActive = false;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Company deactivated" });
        }
    }
}
