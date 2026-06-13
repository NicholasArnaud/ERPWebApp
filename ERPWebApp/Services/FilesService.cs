using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;
using ERPWebApp.Services.IServices;
using ImageMagick;

namespace ERPWebApp.Services
{
    public class FilesService : Service<Files>, IFilesService
    {
        IUnitOfWork _unitOfWork;
        public FilesService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<string> UploadToAzureAsync(IFormFile upload, FileType fileType)
        {
            return await _unitOfWork.AzureBlobStorage.UploadFileAsync(upload, fileType);
        }

        public async Task<string> UploadToAzureAsync(string contentType, string givenFileName, byte[] file, FileType fileType)
        {
            return await _unitOfWork.AzureBlobStorage.UploadFileAsync(contentType, givenFileName , file,fileType);
        }
        
        public async Task<string> UploadToAzureAsync(byte[] content, string fileName, FileType fileType, string contentType = null)
        {
            return await _unitOfWork.AzureBlobStorage.UploadFileAsync(content, fileName, fileType, contentType);
        }

        public async Task<string> UploadThumbnailToAzureAsync(IFormFile upload)
        {
            try
            {
                var fileName = System.IO.Path.GetFileName(upload.FileName);
                using (var stream = upload.OpenReadStream())
                using (var img = new MagickImage(stream))
                {
                    {
                        img.Thumbnail(new MagickGeometry(100, 100));
                        using (var ms = new MemoryStream())
                        {
                            img.Write(ms, MagickFormat.Png);
                            var newcontent = ms.ToArray();
                            var newFileName = Path.ChangeExtension(fileName, ".png");
                            return await _unitOfWork.AzureBlobStorage.UploadFileAsync(newcontent, newFileName, FileType.Thumbnail, "image/png");
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<string> UploadThumbnailToAzureAsync(byte[] content, string fileName)
        {
            try
            {
                using (var img = new MagickImage(content))
                {
                    {
                        img.Thumbnail(new MagickGeometry(100, 100));
                        using (var ms = new MemoryStream())
                        {
                            img.Write(ms, MagickFormat.Png);
                            var newcontent = ms.ToArray();
                            var newFileName = Path.ChangeExtension(fileName, ".png");
                            return await _unitOfWork.AzureBlobStorage.UploadFileAsync(newcontent, newFileName, FileType.Thumbnail, "image/png");
                        }
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> RemoveAzureBlobAsync(string fileurl, FileType fileType)
        {
            return await _unitOfWork.AzureBlobStorage.RemoveFileAsync(fileurl, fileType);
        }
    }
}