namespace ECommerce.Application.Interfaces.Infrastructure
{
    public interface IImageService
    {
        Task<string> UploadImageAsync(Stream imageStream, string fileName, string folderPath = "images/products");
        void DeleteImage(string imageUrl);
    }
}
