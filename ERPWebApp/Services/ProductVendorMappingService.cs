using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Models.Mappings;
using ERPWebApp.Services.IServices;

namespace ERPWebApp.Services
{
    public class ProductVendorMappingService(IUnitOfWork unitOfWork) : Service<ProductVendorMapping>(unitOfWork), IProductVendorMappingService
    {
        readonly IUnitOfWork _unitOfWork = unitOfWork;

        /// <summary>
        /// Creates a new <see cref="ProductVendorMapping"/> entity asynchronously.
        /// </summary>
        /// <param name="entity">The <see cref="ProductVendorMapping"/> entity to be created.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown when an error occurs during the creation process.</exception>
        public override async Task<ProductVendorMapping> AddAsync(ProductVendorMapping entity)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                if (entity.isPrimaryVendor)
                {
                    var existingMapping = await _unitOfWork.ProductVendorMappings.GetListByFilterAsync(x => x.ProductId == entity.ProductId && x.isPrimaryVendor);
                    foreach (var item in existingMapping)
                    {
                        item.isPrimaryVendor = false;
                        _unitOfWork.ProductVendorMappings.Update(item);
                        await _unitOfWork.SaveChangesAsync();
                    }
                }

                await _unitOfWork.ProductVendorMappings.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitAsync();

                return entity;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public Task<IEnumerable<Vendor>> GetVendorsAsync()
        {
            return _unitOfWork.ProductVendorMappings.GetVendorsAsync();
        }

        /// <summary>
        /// Deletes a <see cref="ProductVendorMapping"/> entity asynchronously by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the <see cref="ProductVendorMapping"/> entity to be deleted.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown when an error occurs during the deletion process.</exception>
        public override async Task<int> RemoveAsync(int id)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                await _unitOfWork.ProductVendorMappings.DeleteAsync(id);
                var status = await _unitOfWork.SaveChangesAsync();
                var container = await _unitOfWork.ProductContainers.FilterOneAsync(x => x.ProductVendorMappingId == id);
                if (container != null)
                {
                    await _unitOfWork.ProductContainers.DeleteAsync(container.ContainerId);
                    await _unitOfWork.SaveChangesAsync();
                }
                await _unitOfWork.CommitAsync();
                return status;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Updates a <see cref="ProductVendorMapping"/> entity asynchronously.
        /// </summary>
        /// <param name="entity">The <see cref="ProductVendorMapping"/> entity to be updated.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown when an error occurs during the update process.</exception> 
        public override async Task<int> UpdateAsync(ProductVendorMapping entity)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                if (entity.isPrimaryVendor)
                {
                    var vMaps = await _unitOfWork.ProductVendorMappings.GetListByFilterAsync(
                        x => x.ProductId == entity.ProductId
                            && x.ProductVendorMappingId != entity.ProductVendorMappingId
                            && x.isPrimaryVendor
                    );

                    foreach (var vMap in vMaps)
                    {
                        vMap.isPrimaryVendor = false;
                        _unitOfWork.ProductVendorMappings.Update(vMap);
                        await _unitOfWork.SaveChangesAsync();
                    }
                }

                _unitOfWork.ProductVendorMappings.Update(entity);
                var status = await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitAsync();
                return status;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
    }
}