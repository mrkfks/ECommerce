namespace ECommerce.Application.Interfaces
{
    public interface IFileUploadService
    {
        Task<string> UploadImageAsync(byte[] fileBytes, string fileName, string folderPath);
        Task<bool> DeleteImageAsync(string filePath);
        bool IsValidImageFile(string fileName);
    }
}
