using ERPWebApp.Data.DTOModels;
using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;
using ERPWebApp.Models.Mappings;
using ERPWebApp.Services.IServices;
using Newtonsoft.Json.Linq;
using ERPWebApp.Data;

namespace ERPWebApp.Services
{
    public class ShipStationStoreService : Service<ShipStationStore>, IShipStationStoreService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DateTime _now;
        private readonly ILogger<ShipStationStoreService> _logger;

        public ShipStationStoreService(IUnitOfWork unitOfWork,IHttpClientFactory httpClientFactory, ILogger<ShipStationStoreService> logger) : base(unitOfWork)
        {
            _httpClientFactory = httpClientFactory;
            _unitOfWork = unitOfWork;
            _now = TimeZoneInfo.ConvertTime(
               DateTime.Now,
               TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time")
            );
            _logger = logger;
        }

        public JObject GetShipStationStorePieChartsData()
        {
            return _unitOfWork.ShipStationStores.GetShipStationStorePieChartsData(_now.Date);
        }
        public async Task<ShipStationStore> GetShipStationStoreByEmailAsync(string email)
        {
            return await _unitOfWork.ShipStationStores.GetShipStationStoreByEmailAsync(email);
        }

        public async Task<List<ShipStationStore>> GetAllOrderedByNameAsync()
        {
            return await _unitOfWork.ShipStationStores.GetAllOrderedByNameAsync();
        }

        public async Task<ShipStationStore> GetFirstOrderedByNameAsync()
        {
            return await _unitOfWork.ShipStationStores.GetFirstOrderedByNameAsync();
        }

        public override async Task<ShipStationStore> AddAsync(ShipStationStore entity)
        {
            var fileUrls = new Dictionary<string, FileType>();
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                await _unitOfWork.ShipStationStores.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                if (entity.RawFiles != null && entity.RawFiles.Count > 0)
                {
                    foreach (var RawFile in entity.RawFiles)
                    {
                        if (RawFile is { Length: > 0 })
                        {
                            string extension = Path.GetExtension(RawFile.FileName).ToLower();

                            var type = FileType.Other;
                            if (extension == ".pdf")
                            {
                                type = FileType.Pdf;
                            }
                            else if (extension == ".jpg" || extension == ".png" || extension == ".jpeg")
                            {
                                type = FileType.Image;
                            }

                            var fileUrl = await _unitOfWork.AzureBlobStorage.UploadFileAsync(RawFile, type);

                            fileUrls.Add(fileUrl, type);

                            var file = new Files
                            {
                                FileName = Path.GetFileName(RawFile.FileName),
                                FileType = type,
                                ContentType = RawFile.ContentType,
                                IsThumbnail = false,
                                IsDetailed = true,
                                FileUrl = fileUrl
                            };

                            await _unitOfWork.Files.AddAsync(file);
                            await _unitOfWork.SaveChangesAsync();

                            await _unitOfWork.ShipStationStores.AddStoreFileAsync(new ShipStationStoreFile
                            {
                                FileId = file.FileId,
                                ShipStationStoreId = entity.ShipStationStoreId
                            });
                            await _unitOfWork.SaveChangesAsync();
                        }
                    }
                }

                await _unitOfWork.CommitAsync();

                return entity;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                if (fileUrls.Count > 0)
                {
                    foreach (var fileUrl in fileUrls)
                    {
                        await _unitOfWork.AzureBlobStorage.RemoveFileAsync(fileUrl.Key, fileUrl.Value);
                    }
                }
                _logger.LogError(ex, "Error in AddAsync");
                return null;
            }
        }

        public override async Task<int> UpdateAsync(ShipStationStore entity)
        {
            var fileUrls = new Dictionary<string, FileType>();
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                _unitOfWork.ShipStationStores.Update(entity);
                await _unitOfWork.SaveChangesAsync();

                if (entity.RawFiles != null && entity.RawFiles.Count > 0)
                {
                    foreach (var RawFile in entity.RawFiles)
                    {
                        if (RawFile is { Length: > 0 })
                        {
                            string extension = Path.GetExtension(RawFile.FileName).ToLower();

                            var type = FileType.Other;
                            if (extension == ".pdf")
                            {
                                type = FileType.Pdf;
                            }
                            else if (extension == ".jpg" || extension == ".png" || extension == ".jpeg")
                            {
                                type = FileType.Image;
                            }

                            var fileUrl = await _unitOfWork.AzureBlobStorage.UploadFileAsync(RawFile, type);

                            fileUrls.Add(fileUrl, type);

                            var file = new Files
                            {
                                FileName = Path.GetFileName(RawFile.FileName),
                                FileType = type,
                                ContentType = RawFile.ContentType,
                                IsThumbnail = false,
                                IsDetailed = true,
                                FileUrl = fileUrl
                            };

                            await _unitOfWork.Files.AddAsync(file);
                            await _unitOfWork.SaveChangesAsync();

                            await _unitOfWork.ShipStationStores.AddStoreFileAsync(new ShipStationStoreFile
                            {
                                FileId = file.FileId,
                                ShipStationStoreId = entity.ShipStationStoreId
                            });
                            await _unitOfWork.SaveChangesAsync();
                        }
                    }
                }

                await _unitOfWork.CommitAsync();

                return 1;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                if (fileUrls.Count > 0)
                {
                    foreach (var fileUrl in fileUrls)
                    {
                        await _unitOfWork.AzureBlobStorage.RemoveFileAsync(fileUrl.Key, fileUrl.Value);
                    }
                }
                _logger.LogError(ex, "Error in UpdateAsync");
                return 0;
            }
        }

        public async Task DeleteStoreFile(ShipStationStoreFile storeFile)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                await _unitOfWork.ShipStationStores.DeleteStoreFileAsync(storeFile.StoreFileId);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.Files.DeleteAsync(storeFile.FileId);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.AzureBlobStorage.RemoveFileAsync(storeFile.Files.FileUrl, storeFile.Files.FileType);

                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error in DeleteStoreFile");
                return;
            }
        }

        public async Task<ShipStationStoreFile> GetStoreFileAsync(int storeFileId)
        {
            return await _unitOfWork.ShipStationStores.GetStoreFileAsync(storeFileId);
        }

        public async Task<List<string>> VerifyShipStationStores(IEnumerable<ShipStationJson> shipStations)
        {
            try
            {
                //Extract ShipStation names
                var stores = shipStations.Select(x=> x.StoreName.Replace(" ", "").ToLower())
                    .Distinct()
                    .ToList();

                //Retrieve existing ShipStationStores by filtering the ShipStation names and Ids
                var existingStores = await _unitOfWork.ShipStationStores.GetListByQueryAsync(
                    s =>s.Where(x => stores.Contains(x.StoreName.Replace(" ", "").ToLower()))
                );


                List<ShipStationStore> newStores = [];
                var isUpdate = false;
                List<string> messages = [];

                foreach (var shipStation in shipStations)
                {
                    /*
                    Find the existing ShipStation with the ShipStationName
                    Assume there is no duplicate records
                    */
                    var existingStore = existingStores.FirstOrDefault(
                        x => string.Equals(
                            x.StoreName.Replace(" ", ""),
                            shipStation.StoreName.Replace(" ", ""),
                            StringComparison.CurrentCultureIgnoreCase
                        )
                    ) ?? new ShipStationStore
                    {
                        StoreName = shipStation.StoreName,
                        StoreId = shipStation.StoreId,
                        Email = shipStation.Email,
                        PublicEmail = shipStation.PublicEmail,
                        IsActive = shipStation.IsActive,
                    };

                    if (existingStore.ShipStationStoreId == 0)
                    {
                        var isExists = newStores.Exists(x =>
                            x.StoreName == shipStation.StoreName
                            || x.StoreId == shipStation.StoreId
                        );
                        
                        if (!isExists)
                        {
                            //Collect new ShipStationStores
                            newStores.Add(existingStore);
                        }
                    }else if (existingStore.StoreId != shipStation.StoreId)
                    {
                        var isExists = await _unitOfWork.ShipStationStores.IsExistsAsync(
                            x => x.StoreId == shipStation.StoreId
                        );
                        if (!isExists)
                        {
                            existingStore.StoreId = shipStation.StoreId;
                            _unitOfWork.ShipStationStores.Update(existingStore);
                            isUpdate = true;
                        }
                        else
                        {
                            messages.Add($"Store {shipStation.StoreName} already exists.");
                        }
                    }
                }

                if (isUpdate) await _unitOfWork.SaveChangesAsync();

                if (newStores.Count <= 0) return messages;
                //Insert all new ShipStationStores as bulk
                await _unitOfWork.ShipStationStores.AddRangeAsync(newStores);
                await _unitOfWork.SaveChangesAsync();

                return messages;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in VerifyShipStationStores");
                throw;
            }
        }


        public async Task<IEnumerable<ShipStationJson>> GetShipStationStores()
        {
            using HttpClient client = _httpClientFactory.CreateClient("ShipStationV1");
            var response = await client.GetFromJsonAsync<List<ShipStationJson>>("stores");
            return response;
        }
    }
}
