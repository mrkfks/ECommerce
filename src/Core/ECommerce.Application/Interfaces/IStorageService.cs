using System.IO;

namespace ECommerce.Application.Interfaces
{
    public interface IStorageService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string folder);
        Task DeleteFileAsync(string fileUrl);
    }
}

