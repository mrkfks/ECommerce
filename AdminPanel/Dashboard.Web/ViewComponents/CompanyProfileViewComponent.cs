using Microsoft.AspNetCore.Mvc;
using Dashboard.Web.Services;
using ECommerce.Application.DTOs;

namespace Dashboard.Web.ViewComponents
{
    public class CompanyProfileViewComponent : ViewComponent
    {
        private readonly IApiService<CompanyDto> _companyService;
        private readonly UserApiService _userService;

        public CompanyProfileViewComponent(IApiService<CompanyDto> companyService, UserApiService userService)
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
            CompanyDto? company = null;

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
