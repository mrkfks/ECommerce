using Dashboard.Web.Services;
using ECommerce.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dashboard.Web.Controllers
{
    [Authorize(Roles = "CompanyAdmin,SuperAdmin,User")]
    public class ReviewController : Controller
    {
        private readonly ReviewApiService _reviewService;

        public ReviewController(ReviewApiService reviewService)
        {
            _reviewService = reviewService;
        }

        // Listeleme
        public async Task<IActionResult> Index()
        {
            var reviews = await _reviewService.GetAllAsync();
            return View(reviews.Data ?? new List<ReviewDto>());
        }

        // Detay
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var review = await _reviewService.GetByIdAsync(id);

            if (review == null)
                return NotFound();

            return View(review);
        }

        // Silme
        [Authorize(Roles = "CompanyAdmin,SuperAdmin,User")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _reviewService.GetByIdAsync(id);

            if (review == null)
                return NotFound();

            return View(review);
        }

        [Authorize(Roles = "CompanyAdmin,SuperAdmin,User")]
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await _reviewService.DeleteAsync(id);

            if (response != null && response.Success)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Yorum silinirken hata oluştu.");
            var review = await _reviewService.GetByIdAsync(id);
            return View(review?.Data);
        }
    }
}

