using ERPWebApp.Data.Repositories.Azure.Interface;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using ERPWebApp.Models.Config;
using Azure.Identity;
using ERPWebApp.Models;
using Azure.Storage.Blobs.Models;

namespace ERPWebApp.Data.Repositories.Azure
{
    public class AzureBlobStorageRepository : IAzureBlobStorageRepository
    {
        BlobServiceClient _serviceClient;
        private readonly AzureStorageConfig _storageConfig;

        public AzureBlobStorageRepository(IOptions<AzureStorageConfig> config)
        {
            _storageConfig = config.Value;
            _serviceClient = GetBlobServiceClient();
        }

        private BlobServiceClient GetBlobServiceClient()
        {
            try
            {
                var isConfigured = String.IsNullOrEmpty(_storageConfig.AccountName)
                    || String.IsNullOrEmpty(_storageConfig.ImageContainer)
                    || String.IsNullOrEmpty(_storageConfig.DocumentContainer)
                    || String.IsNullOrEmpty(_storageConfig.ThumbnailContainer)
                    || String.IsNullOrEmpty(_storageConfig.DHLInvoiceContainer)
                    || String.IsNullOrEmpty(_storageConfig.UPSInvoiceContainer)
                    || String.IsNullOrEmpty(_storageConfig.StampsUSPSInvoiceContainer)
                    || String.IsNullOrEmpty(_storageConfig.EasyPostInvoiceContainer)
                    || String.IsNullOrEmpty(_storageConfig.SkulabsImportContainer)
                    || String.IsNullOrEmpty(_storageConfig.DefaultContainer);

                if (isConfigured) throw new Exception("Azure Blob Storage not configured");
                return new(new Uri($"https://{_storageConfig.AccountName}.blob.core.windows.net"), new DefaultAzureCredential());
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<BlobContainerClient> GetContainerClientAsync(string containerName)
        {
            try
            {
                var container = _serviceClient.GetBlobContainerClient(containerName);
                var exists = await container.ExistsAsync();
                if (!exists)
                    await container.CreateIfNotExistsAsync(publicAccessType: PublicAccessType.Blob);
                return container;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private BlobContainerClient GetContainerClient(string containerName)
        {
            try
            {
                var container = _serviceClient.GetBlobContainerClient(containerName);
                if (!container.Exists())
                    container.CreateIfNotExists(publicAccessType: PublicAccessType.Blob);
                return container;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string GetContainer(FileType fileType)
            => fileType switch
            {
                FileType.Image => _storageConfig.ImageContainer,
                FileType.Pdf => _storageConfig.DocumentContainer,
                FileType.Thumbnail => _storageConfig.ThumbnailContainer,
                FileType.DHLInvoice => _storageConfig.DHLInvoiceContainer,
                FileType.UPSInvoice => _storageConfig.UPSInvoiceContainer,
                FileType.StampsUSPSInvoice => _storageConfig.StampsUSPSInvoiceContainer,
                FileType.EasyPostInvoice => _storageConfig.EasyPostInvoiceContainer,
                FileType.SkulabsImport => _storageConfig.SkulabsImportContainer,
                FileType.ShippingManifests => _storageConfig.ShippingManifests,
                _ => _storageConfig.DefaultContainer
            };

        public async Task<string> UploadFileAsync(IFormFile file, FileType fileType)
        {
            try
            {
                var fileName = Guid.NewGuid().ToString() + file.FileName;
                string container = GetContainer(fileType);
                var containerClient = await GetContainerClientAsync(container);
                BlobClient blobClient = containerClient.GetBlobClient(fileName);
                BlobUploadOptions options = new()
                {
                    HttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType }
                };

                using var reader = new System.IO.BinaryReader(file.OpenReadStream());
                var content = reader.ReadBytes((int)file.Length);
                BinaryData binaryData = new(content);
                await blobClient.UploadAsync(binaryData, options);
                return blobClient.Uri.ToString();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<string> UploadFileAsync(string contentType,string givenFileName,byte[] file, FileType fileType)
        {
            try
            {
                var fileName = Guid.NewGuid().ToString() + givenFileName;
                string container = GetContainer(fileType);
                var containerClient = await GetContainerClientAsync(container);
                BlobClient blobClient = containerClient.GetBlobClient(fileName);
                BlobUploadOptions options = new()
                {
                    HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
                };

               
                var content = contentType;
                BinaryData binaryData = new(content);
                await blobClient.UploadAsync(binaryData, options);
                return blobClient.Uri.ToString();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<string> UploadFileAsync(byte[] content, string fileName, FileType fileType, string contentType = null)
        {
            try
            {
                fileName = Guid.NewGuid().ToString() + fileName;
                string container = GetContainer(fileType);
                var containerClient = await GetContainerClientAsync(container);
                BlobClient blobClient = containerClient.GetBlobClient(fileName);
                BlobUploadOptions options = null;
                if (contentType != null)
                {
                    options = new BlobUploadOptions
                    {
                        HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
                    };
                }

                BinaryData binaryData = new(content);
                await blobClient.UploadAsync(binaryData, options);
                return blobClient.Uri.ToString();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string UploadFile(IFormFile file, FileType fileType)
        {
            try
            {
                var fileName = Guid.NewGuid().ToString() + file.FileName;
                string container = GetContainer(fileType);
                var containerClient = GetContainerClient(container);
                BlobClient blobClient = containerClient.GetBlobClient(fileName);
                BlobUploadOptions options = new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType }
                };
                using (var reader = new System.IO.BinaryReader(file.OpenReadStream()))
                {
                    var content = reader.ReadBytes((int)file.Length);
                    BinaryData binaryData = new BinaryData(content);
                    blobClient.Upload(binaryData, options);
                    return blobClient.Uri.ToString();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string UploadFile(byte[] content, string fileName, FileType fileType, string contentType = null)
        {
            try
            {
                fileName = Guid.NewGuid().ToString() + fileName;
                string container = GetContainer(fileType);
                var containerClient = GetContainerClient(container);
                BlobClient blobClient = containerClient.GetBlobClient(fileName);
                BlobUploadOptions options = null;
                if (contentType != null)
                {
                    options = new BlobUploadOptions
                    {
                        HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
                    };
                }
                BinaryData binaryData = new BinaryData(content);
                blobClient.Upload(binaryData, options);
                return blobClient.Uri.ToString();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> RemoveFileAsync(string fileName, FileType fileType)
        {
            try
            {
                string container = GetContainer(fileType);
                var containerClient = GetContainerClient(container);
                BlobClient blobClient = containerClient.GetBlobClient(fileName);
                return await blobClient.DeleteIfExistsAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool RemoveFile(string fileName, FileType fileType)
        {
            try
            {
                string container = GetContainer(fileType);
                var containerClient = GetContainerClient(container);
                BlobClient blobClient = containerClient.GetBlobClient(fileName);
                return blobClient.DeleteIfExists();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<bool> FileExistsAsync(string fileName, FileType fileType)
        {
            try
            {
                string container = GetContainer(fileType);
                var containerClient = await GetContainerClientAsync(container);

                await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
                {
                    if (blobItem.Name.Contains(fileName))
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
