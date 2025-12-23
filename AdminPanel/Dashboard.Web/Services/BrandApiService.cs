using ECommerce.Application.DTOs;

namespace Dashboard.Web.Services;

public class BrandApiService : ApiService<BrandDto>
{
    public BrandApiService(HttpClient httpClient) 
        : base(httpClient, "Brand")
    {
    }
}
