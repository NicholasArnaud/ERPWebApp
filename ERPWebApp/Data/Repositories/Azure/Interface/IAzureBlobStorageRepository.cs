using ERPWebApp.Models;
namespace ERPWebApp.Data.Repositories.Azure.Interface
{
    public interface IAzureBlobStorageRepository
    {
        Task<string> UploadFileAsync(IFormFile file, FileType fileType);
        Task<string> UploadFileAsync(byte[] content, string fileName, FileType fileType, string contentType = null);
        string UploadFile(IFormFile file, FileType fileType);
        string UploadFile(byte[] content, string fileName, FileType fileType, string contentType = null);
        Task<bool> RemoveFileAsync(string fileName, FileType fileType);
        bool RemoveFile(string fileName, FileType fileType);
        Task<bool> FileExistsAsync(string fileName, FileType fileType);
        Task<string> UploadFileAsync(string contentType, string givenFileName, byte[] file, FileType fileType);
    }
}
