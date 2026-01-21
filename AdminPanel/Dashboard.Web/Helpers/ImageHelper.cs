using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dashboard.Web.Helpers;

public static class ImageHelper
{
    private static string? _apiBaseUrl;

    public static void Configure(IConfiguration configuration)
    {
        _apiBaseUrl = configuration["ApiSettings:BaseUrl"];
    }

    public static string GetImageUrl(this IHtmlHelper helper, string? imagePath)
    {
        if (string.IsNullOrEmpty(imagePath))   
            return "/assets/img/default-product.png"; // Varsayılan resim

        if (imagePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            return imagePath;

        // Ensure proper slash handling
        var cleanPath = imagePath.TrimStart('~', '/');
        
        // Return absolute URL from API
        return $"{_apiBaseUrl?.TrimEnd('/')}/{cleanPath}";
    }
}
