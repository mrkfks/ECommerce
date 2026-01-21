using ECommerce.Application.DTOs;

namespace Dashboard.Web.Services
{
    public class ProductApiService : ApiService<ProductDto>
    {
        public ProductApiService(IHttpClientFactory httpClientFactory) : base(httpClientFactory)
        {
        }
    }
}
