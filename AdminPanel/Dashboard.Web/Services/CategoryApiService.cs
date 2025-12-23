using ECommerce.Application.DTOs;

namespace Dashboard.Web.Services;

public class CategoryApiService : ApiService<CategoryDto>
{
    public CategoryApiService(HttpClient httpClient) 
        : base(httpClient, "Category")
    {
    }
}
