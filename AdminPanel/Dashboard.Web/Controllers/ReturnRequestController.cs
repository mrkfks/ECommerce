using System.Net.Http.Json;
using Dashboard.Web.Services;
using ECommerce.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
                    // ApiResponseDto wrapper'dan data çıkar
                    using var jsonDoc = System.Text.Json.JsonDocument.Parse(json);
                    var root = jsonDoc.RootElement;

                    if (root.TryGetProperty("data", out var dataElement))
                    {
                        var dataJson = dataElement.GetRawText();
                        // Case-insensitive deserialization için options ekle
                        var options = new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                            PropertyNameCaseInsensitive = true,
                            WriteIndented = true
                        };

                        returnRequests = System.Text.Json.JsonSerializer.Deserialize<List<ReturnRequestDto>>(dataJson, options) ?? new List<ReturnRequestDto>();
                        foreach (var req in returnRequests)
                        {
                        }
                    }
                    else
                    {
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    // Her durumda error detaylarını loggala
                    if (string.IsNullOrEmpty(errorContent))
                    {
                        errorContent = "[Empty response body]";
                    }

                    if (errorContent.StartsWith("<"))
                    {
                    }
                }

                return View(returnRequests);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception caught: {ex.Message}");
                return View(new List<ReturnRequestDto>());
            }
        }

        [HttpPost("{id}/update-status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateReturnRequestDto dto)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();

                // Token'ı Cookie'den al
                var token = HttpContext.Request.Cookies["AuthToken"];
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                // Tenant header ekle
                if (HttpContext.User.FindFirst("CompanyId") is { } companyClaim)
                {
                    client.DefaultRequestHeaders.Add("X-Company-Id", companyClaim.Value);
                }

                var apiUrl = $"http://localhost:5010/api/return-requests/{id}/status";
                var requestJson = System.Text.Json.JsonSerializer.Serialize(dto);
                var response = await client.PutAsJsonAsync(apiUrl, dto);
                var content = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var result = new { success = true, message = "İade talebi durumu başarıyla güncellendi" };
                    var resultJson = System.Text.Json.JsonSerializer.Serialize(result);
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
                        Console.WriteLine($"Exception caught: {parseEx.Message}");
                    }
                }

                var errorResult = new { success = false, message = errorMessage ?? "Güncelleme başarısız oldu" };
                var errorJson = System.Text.Json.JsonSerializer.Serialize(errorResult);
                return Ok(errorResult);
            }
            catch (Exception ex)
            {
                var errorResult = new { success = false, message = ex.Message };
                var errorJson = System.Text.Json.JsonSerializer.Serialize(errorResult);
                return Ok(errorResult);
            }
        }
    }
}
