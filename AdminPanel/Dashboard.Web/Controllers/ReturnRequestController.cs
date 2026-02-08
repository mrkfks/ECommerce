using ECommerce.Application.DTOs;
using Dashboard.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace Dashboard.Web.Controllers
{
    [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
    [Route("[controller]")]
    public class ReturnRequestController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ReturnRequestController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                // Token'ı Cookie'den al
                var token = HttpContext.Request.Cookies["AuthToken"];
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                
                // Admin bypass header ekle (development için)
                client.DefaultRequestHeaders.Add("X-Admin-Bypass", "true");
                
                // Tenant header ekle
                if (HttpContext.User.FindFirst("CompanyId") is { } companyClaim)
                {
                    client.DefaultRequestHeaders.Add("X-Company-Id", companyClaim.Value);
                }
                
                var response = await client.GetAsync("http://localhost:5010/api/return-requests");
                var returnRequests = new List<ReturnRequestDto>();
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[ReturnRequestController] Raw JSON: {json}");
                    
                    // ApiResponseDto wrapper'dan data çıkar
                    using var jsonDoc = System.Text.Json.JsonDocument.Parse(json);
                    var root = jsonDoc.RootElement;
                    
                    if (root.TryGetProperty("data", out var dataElement))
                    {
                        var dataJson = dataElement.GetRawText();
                        Console.WriteLine($"[ReturnRequestController] Data JSON: {dataJson}");
                        
                        // Case-insensitive deserialization için options ekle
                        var options = new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                            PropertyNameCaseInsensitive = true,
                            WriteIndented = true
                        };
                        
                        returnRequests = System.Text.Json.JsonSerializer.Deserialize<List<ReturnRequestDto>>(dataJson, options) ?? new List<ReturnRequestDto>();
                        Console.WriteLine($"[ReturnRequestController] Parsed {returnRequests.Count} return requests");
                        foreach (var req in returnRequests)
                        {
                            Console.WriteLine($"  - ID: {req.Id}, CustomerName: {req.CustomerName}, OrderId: {req.OrderId}, ProductName: {req.ProductName}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[ReturnRequestController] NO 'data' property found in response!");
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[ReturnRequestController] API Error: {response.StatusCode} - {errorContent}");
                    
                    // Her durumda error detaylarını loggala
                    if (string.IsNullOrEmpty(errorContent))
                    {
                        errorContent = "[Empty response body]";
                    }
                    
                    if (errorContent.StartsWith("<"))
                    {
                        Console.WriteLine($"[ReturnRequestController] HTML Error Page Received - likely authentication/authorization failure");
                    }
                }
                
                return View(returnRequests);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ReturnRequestController] Error: {ex.Message}\n{ex.StackTrace}");
                return View(new List<ReturnRequestDto>());
            }
        }

        [HttpPost("{id}/update-status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateReturnRequestDto dto)
        {
            try
            {
                Console.WriteLine($"[UpdateStatus] Called with ID: {id}");
                Console.WriteLine($"[UpdateStatus] DTO Status: {dto.Status}");
                Console.WriteLine($"[UpdateStatus] DTO AdminResponse: {dto.AdminResponse}");
                
                var client = _httpClientFactory.CreateClient();
                
                // Token'ı Cookie'den al
                var token = HttpContext.Request.Cookies["AuthToken"];
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    Console.WriteLine($"[UpdateStatus] Token found, length: {token.Length}");
                }
                
                // Tenant header ekle
                if (HttpContext.User.FindFirst("CompanyId") is { } companyClaim)
                {
                    client.DefaultRequestHeaders.Add("X-Company-Id", companyClaim.Value);
                    Console.WriteLine($"[UpdateStatus] Company ID: {companyClaim.Value}");
                }
                
                var apiUrl = $"http://localhost:5010/api/return-requests/{id}/status";
                Console.WriteLine($"[UpdateStatus] API URL: {apiUrl}");
                
                var requestJson = System.Text.Json.JsonSerializer.Serialize(dto);
                Console.WriteLine($"[UpdateStatus] Request JSON: {requestJson}");
                
                var response = await client.PutAsJsonAsync(apiUrl, dto);
                var content = await response.Content.ReadAsStringAsync();
                
                Console.WriteLine($"[UpdateStatus] Response Status: {response.StatusCode}");
                Console.WriteLine($"[UpdateStatus] Response Content Length: {content?.Length ?? 0}");
                Console.WriteLine($"[UpdateStatus] Response Content: {content}");
                
                if (response.IsSuccessStatusCode)
                {
                    var result = new { success = true, message = "İade talebi durumu başarıyla güncellendi" };
                    var resultJson = System.Text.Json.JsonSerializer.Serialize(result);
                    Console.WriteLine($"[UpdateStatus] Returning success: {resultJson}");
                    return Ok(result);
                }
                
                // Hata durumunda dönen veriyi parse et
                var errorMessage = "";
                if (!string.IsNullOrEmpty(content))
                {
                    try
                    {
                        using var jsonDoc = System.Text.Json.JsonDocument.Parse(content);
                        var root = jsonDoc.RootElement;
                        if (root.TryGetProperty("message", out var msgElement))
                            errorMessage = msgElement.GetString() ?? "Bilinmeyen hata";
                    }
                    catch (Exception parseEx)
                    {
                        Console.WriteLine($"[UpdateStatus] JSON Parse Error: {parseEx.Message}");
                        errorMessage = content;
                    }
                }
                
                var errorResult = new { success = false, message = errorMessage ?? "Güncelleme başarısız oldu" };
                var errorJson = System.Text.Json.JsonSerializer.Serialize(errorResult);
                Console.WriteLine($"[UpdateStatus] Returning error: {errorJson}");
                return Ok(errorResult);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UpdateStatus] Exception: {ex.Message}");
                Console.WriteLine($"[UpdateStatus] Stack: {ex.StackTrace}");
                var errorResult = new { success = false, message = ex.Message };
                var errorJson = System.Text.Json.JsonSerializer.Serialize(errorResult);
                Console.WriteLine($"[UpdateStatus] Returning exception: {errorJson}");
                return Ok(errorResult);
            }
        }
    }
}
