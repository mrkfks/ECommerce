using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.RestApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "SameCompanyOrSuperAdmin")]
public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }
        //CREATE
        [HttpPost]
        [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
        public async Task<IActionResult> Add(ReviewCreateDto dto)
        {
            var review = await _reviewService.CreateAsync(dto);
            return Ok(review);
        }
        //READ - Get By Id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var review = await _reviewService.GetByIdAsync(id);
            if (review == null)
                return NotFound(new { message = "Yorum Bulunamadı" });
            return Ok(review);
        }
        // READ - Get All
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var reviews = await _reviewService.GetAllAsync();
            return Ok(reviews);
        }

        //READ - Get by Product
        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GEtByProduct(int productId)
        {
            var reviews = await _reviewService.GetByProductIdAsync(productId);
            return Ok(reviews);
        }
        // READ - Get by Customer
        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetByCustomer(int customerId)
        {
            var reviews = await _reviewService.GetByCustomerIdAsync(customerId);
            return Ok(reviews);
        }

        // UPDATE
        [HttpPut("{id}")]
        [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
        public async Task<IActionResult> Update(int id, ReviewUpdateDto dto)
        {
            if (id != dto.Id) return BadRequest();
            await _reviewService.UpdateAsync(dto);
            return Ok(new { message = "Yorum güncellendi." });
        }

        // DELETE
        [HttpDelete("{id}")]
        [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _reviewService.DeleteAsync(id);
            return Ok(new { message = "Yorum başarıyla silindi." });
        }
}