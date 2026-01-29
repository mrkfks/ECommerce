using Dashboard.Web.Models;

namespace Dashboard.Web.Services
{
    public class CompanyApiService : ApiService<CompanyDto>
    {
        public CompanyApiService(HttpClient httpClient) : base(httpClient)
        {
        }
    }
}
