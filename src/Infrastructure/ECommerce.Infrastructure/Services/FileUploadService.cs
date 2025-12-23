using ECommerce.Application.Interfaces;

namespace ECommerce.Infrastructure.Services;

public class FileUploadService : IFileUploadService
{
    private readonly string _uploadsFolder;
    private readonly List<string> _allowedExtensions = new() { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private readonly long _maxFileSize = 5 * 1024 * 1024; // 5 MB

    public FileUploadService(string uploadsFolder)
    {
        _uploadsFolder = uploadsFolder;
        
        // Uploads klasörü yoksa oluştur
        if (!Directory.Exists(_uploadsFolder))
        {
            Directory.CreateDirectory(_uploadsFolder);
        }
    }

    public async Task<string> UploadImageAsync(byte[] fileBytes, string fileName, string folderPath)
    {
        if (!IsValidImageFile(fileName))
        {
            throw new InvalidOperationException("Geçersiz dosya formatı. Sadece JPG, PNG, GIF, WEBP dosyaları kabul edilir.");
        }

        if (fileBytes.Length > _maxFileSize)
        {
            throw new InvalidOperationException($"Dosya boyutu {_maxFileSize / (1024 * 1024)} MB'dan büyük olamaz.");
        }

        // Klasörü oluştur
        string uploadPath = Path.Combine(_uploadsFolder, folderPath);
        if (!Directory.Exists(uploadPath))
        {
            Directory.CreateDirectory(uploadPath);
        }

        // Benzersiz dosya adı oluştur
        string uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        string filePath = Path.Combine(uploadPath, uniqueFileName);

        try
        {
            await File.WriteAllBytesAsync(filePath, fileBytes);

            // Web URL'sini döndür
            return $"/uploads/{folderPath}/{uniqueFileName}";
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Dosya yükleme sırasında hata oluştu: {ex.Message}");
        }
    }

    public async Task<bool> DeleteImageAsync(string filePath)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            // URL'den dosya yolunu al
            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath.TrimStart('/'));

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Dosya silme sırasında hata oluştu: {ex.Message}");
        }
    }

    public bool IsValidImageFile(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return false;

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return _allowedExtensions.Contains(extension);
    }
}
