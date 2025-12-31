using ECommerce.Application.DTOs;

namespace Dashboard.Web.Services
{
    public class RequestApiService : ApiService<RequestDto>
    {
        public RequestApiService(HttpClient httpClient) : base(httpClient, "Request")
        {
        }

        public async Task<bool> CreateAsync(RequestCreateDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Request", dto);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ApproveAsync(int id)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/Request/{id}/approve", null);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RejectAsync(int id)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/Request/{id}/reject", null);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<RequestDto>> GetPendingRequestsAsync()
        {
            try
            {
                var requests = await _httpClient.GetFromJsonAsync<List<RequestDto>>("api/Request/pending");
                return requests ?? new List<RequestDto>();
            }
            catch
            {
                return new List<RequestDto>();
            }
        }
    }
}
