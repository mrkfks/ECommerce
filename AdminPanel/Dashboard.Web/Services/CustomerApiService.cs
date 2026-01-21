using ECommerce.Application.DTOs;

namespace Dashboard.Web.Services
{
    public class CustomerApiService : ApiService<CustomerDto>
    {
        public CustomerApiService(IHttpClientFactory httpClientFactory) : base(httpClientFactory)
        {
        }
    }
}
