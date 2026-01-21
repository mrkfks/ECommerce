using ECommerce.Application.DTOs;
using System.Net.Http.Json;

namespace Dashboard.Web.Services;

public class ProductApiService : ApiService<ProductDto>
{
    public ProductApiService(HttpClient httpClient) 
        : base(httpClient, "Product")
    {
    }

    // API ProductCreateDto bekliyor, bu yüzden özel Create metodu
    public async Task<bool> CreateAsync(ProductCreateDto dto)
    {
        try
        {
            Console.WriteLine($"[ProductApiService] Creating product: {dto.Name}, CompanyId: {dto.CompanyId}");
            var response = await _httpClient.PostAsJsonAsync($"api/{_endpoint}", dto);
            Console.WriteLine($"[ProductApiService] Response: {response.StatusCode}");
            
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[ProductApiService] Error content: {content}");
            }
            
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ProductApiService] Exception: {ex.Message}");
            return false;
        }
    }
    public async Task<bool> BulkUpdateAsync(List<int> productIds, decimal percentage)
    {
        try
        {
            var dto = new ProductBulkUpdateDto { ProductIds = productIds, PriceIncreasePercentage = percentage };
            var response = await _httpClient.PostAsJsonAsync($"api/{_endpoint}/bulk-price-update", dto);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ProductApiService] Exception: {ex.Message}");
            return false;
        }
    }
}
