
using ECommerce.Application.DTOs;
using System.IO;

namespace ECommerce.Application.Interfaces
{
    public interface IStorageService
    {
        Task<ImageUploadResultDto> UploadFileAsync(Stream fileStream, string fileName, string folder);
        Task DeleteFileAsync(string fileUrl);
    }
}

