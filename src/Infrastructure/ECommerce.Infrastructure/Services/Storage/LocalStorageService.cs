using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace ECommerce.Infrastructure.Services.Storage
{
    public class LocalStorageService : IStorageService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITenantService _tenantService;

        public LocalStorageService(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor, ITenantService tenantService)
        {
            _env = env;
            _httpContextAccessor = httpContextAccessor;
            _tenantService = tenantService;
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string folder)
        {
            var companyId = _tenantService.GetCompanyId();
            var tenantPathFragment = companyId.HasValue ? companyId.Value.ToString() : "global";

            var webRoot = _env.WebRootPath ?? string.Empty;
            var uploadsFolder = Path.Combine(webRoot, "uploads", tenantPathFragment, folder);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + fileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(stream);
            }

            var request = _httpContextAccessor.HttpContext?.Request;
            var scheme = request?.Scheme ?? "http";
            string host;
            if (request?.Host.HasValue == true)
            {
                host = request.Host.Value;
            }
            else
            {
                host = "localhost";
            }
            var baseUrl = $"{scheme}://{host}";
            return $"{baseUrl}/uploads/{tenantPathFragment}/{folder}/{uniqueFileName}";
        }

        public Task DeleteFileAsync(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl)) return Task.CompletedTask;

            var webRoot = _env.WebRootPath ?? string.Empty;
            try
            {
                // URL örnek: http://localhost:5000/uploads/1/products/abc.jpg
                // WebRootPath: .../wwwroot

                // 1. URL'den path kısmını al
                Uri? uri = null;
                if (!Uri.TryCreate(fileUrl, UriKind.Absolute, out uri))
                {
                    // Belki relative path gelmiştir (/uploads/...)
                    if (fileUrl.StartsWith("/"))
                        uri = new Uri("http://dummy" + fileUrl);
                    else
                        return Task.CompletedTask; // Geçersiz format
                }

                var localPath = uri?.LocalPath ?? string.Empty; // /uploads/1/products/abc.jpg

                // 2. Başındaki /'ı kaldır ve sistem seperatorüne çevir
                var relativePath = (localPath ?? string.Empty).TrimStart('/').Replace('/', Path.DirectorySeparatorChar);

                // 3. Full path oluştur
                var fullPath = Path.Combine(webRoot, relativePath);

                if (!string.IsNullOrEmpty(fullPath) && File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch (Exception)
            {
                // Log and swallow - dosya silinememesi süreci kırmamalı
                // _logger.LogError(ex, "File delete failed: {Url}", fileUrl); (Logger yoksa yut)
            }

            return Task.CompletedTask;
        }
    }
}
