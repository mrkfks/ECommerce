using ECommerce.Application.DTOs;

namespace Dashboard.Web.Services
{
    public class ReviewApiService : ApiService<ReviewDto>
    {
        public ReviewApiService(IHttpClientFactory httpClientFactory) : base(httpClientFactory)
        {
        }
    }
}
