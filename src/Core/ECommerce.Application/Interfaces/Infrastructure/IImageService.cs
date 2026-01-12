using System.IO; // Acceptable in IImageService definition if we wrap it, but cleaner to use Stream. 
// However, IImageService will likely be implemented in Infrastructure which references Web/AspNetCore.
// To keep Core pure, we should use Stream.

namespace ECommerce.Application.Interfaces.Infrastructure
{
    public interface IImageService
    {
        Task<string> UploadImageAsync(Stream imageStream, string fileName, string folderPath = "images/products");
        void DeleteImage(string imageUrl);
    }
}
