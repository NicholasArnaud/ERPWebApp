using ERPWebApp.Models;

namespace ERPWebApp.Services.IServices
{
    public interface IFilesService : IService<Files>
    {
        Task<string> UploadToAzureAsync(IFormFile upload, FileType fileType);
        Task<string> UploadToAzureAsync(byte[] content, string fileName, FileType fileType, string contentType = null);
        Task<string> UploadThumbnailToAzureAsync(IFormFile uploads);
        Task<string> UploadThumbnailToAzureAsync(byte[] content, string fileName);
        Task<bool> RemoveAzureBlobAsync(string fileName, FileType fileType);
        //Task<string>  UploadToAzureAsync(string contentType, string givenFileName, byte[] file, FileType fileType);
    }
}