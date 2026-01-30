using Dashboard.Web.Services;
using ECommerce.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Dashboard.Web.ViewComponents
{
    public class CompanyProfileViewComponent : ViewComponent
    {
        private readonly IApiService<ECommerce.Application.DTOs.CompanyDto> _companyService;
        private readonly UserApiService _userService;

        public CompanyProfileViewComponent(IApiService<ECommerce.Application.DTOs.CompanyDto> companyService, UserApiService userService)
        {
            _companyService = companyService;
            _userService = userService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Kullanıcının şirket bilgisini al
            // Önce user profile'dan companyID'yi bulalım
            // Veya User claims'den okuyabiliriz: User.Claims.FirstOrDefault(c => c.Type == "CompanyId")

            var companyIdClaim = HttpContext.User.FindFirst("CompanyId");
            ECommerce.Application.DTOs.CompanyDto? company = null;

            if (companyIdClaim != null && int.TryParse(companyIdClaim.Value, out int companyId))
            {
                var response = await _companyService.GetByIdAsync(companyId);
                company = response?.Data;
            }

            // Eğer company null ise varsayılan dönebilir veya boş
            return View(company);
        }
    }
}
