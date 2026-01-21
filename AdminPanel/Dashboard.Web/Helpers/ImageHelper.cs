using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dashboard.Web.Helpers;

public static class ImageHelper
{
    private static string? _apiBaseUrl;
    private static string? _apiResourceUrl;

    public static void Configure(IConfiguration configuration)
    {
        _apiBaseUrl = configuration["ApiSettings:BaseUrl"];
        _apiResourceUrl = configuration["ApiSettings:ResourceUrl"] ?? _apiBaseUrl;
    }

    public static string GetImageUrl(string? imagePath)
    {
        if (string.IsNullOrEmpty(imagePath))   
            return "/assets/img/default-product.png"; // Varsayılan resim

        if (imagePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            return imagePath;

        // Ensure proper slash handling
        var cleanPath = imagePath.TrimStart('~', '/');
        
        // Return absolute URL from API (Resource Url)
        return $"{_apiResourceUrl?.TrimEnd('/')}/{cleanPath}";
    }

    // Extension method support (optional)
    public static string GetImageUrl(this IHtmlHelper helper, string? imagePath)
    {
        return GetImageUrl(imagePath);
    }
}
