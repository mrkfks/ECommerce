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
            
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", tenantPathFragment, folder);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + fileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(stream);
            }

            var request = _httpContextAccessor.HttpContext?.Request;
            var baseUrl = $"{request?.Scheme}://{request?.Host}";
            return $"{baseUrl}/uploads/{tenantPathFragment}/{folder}/{uniqueFileName}";
        }

        public Task DeleteFileAsync(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl)) return Task.CompletedTask;

            try 
            {
                // URL örnek: http://localhost:5000/uploads/1/products/abc.jpg
                // WebRootPath: .../wwwroot
                
                // 1. URL'den path kısmını al
                Uri uri;
                if (!Uri.TryCreate(fileUrl, UriKind.Absolute, out uri))
                {
                    // Belki relative path gelmiştir (/uploads/...)
                    if (fileUrl.StartsWith("/")) 
                        uri = new Uri("http://dummy" + fileUrl);
                    else
                        return Task.CompletedTask; // Geçersiz format
                }
                
                var localPath = uri.LocalPath; // /uploads/1/products/abc.jpg
                
                // 2. Başındaki /'ı kaldır ve sistem seperatorüne çevir
                var relativePath = localPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                
                // 3. Full path oluştur
                var fullPath = Path.Combine(_env.WebRootPath, relativePath);
                
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch (Exception ex)
            {
                // Log and swallow - dosya silinememesi süreci kırmamalı
                 // _logger.LogError(ex, "File delete failed: {Url}", fileUrl); (Logger yoksa yut)
            }
            
            return Task.CompletedTask;
        }
    }
}
