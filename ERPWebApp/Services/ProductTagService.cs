using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Services.IServices;

namespace ERPWebApp.Services
{
    public class ProductTagService : Service<ProductTagsRegistry>, IProductTagService
    {
        private IUnitOfWork _unitOfWork;
        public ProductTagService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public void AssignProductTag(ProductTagsRegistry tag, int productId)
        {
            try
            {
                var productTag = new ProductTag { ProductId = productId };
                if (tag.TagId != 0)
                {
                    _unitOfWork.ProductTags.Update(tag);
                    productTag.TagId = tag.TagId;
                }
                else
                {
                    var newTag = _unitOfWork.ProductTags.Add(tag);
                    productTag.TagId = newTag.TagId;
                }

                _unitOfWork.ProductTags.AssignProductTag(productTag);
                _unitOfWork.SaveChanges();
            }
            catch (System.Exception)
            {

                throw;
            }
        }

        public async Task AssignProductTagAsync(ProductTagsRegistry tag, int productId)
        {
            try
            {
                var productTag = new ProductTag { ProductId = productId };
                if (tag.TagId != 0)
                {
                    _unitOfWork.ProductTags.Update(tag);
                    productTag.TagId = tag.TagId;
                }
                else
                {
                    var newTag = await _unitOfWork.ProductTags.AddAsync(tag);
                    productTag.TagId = newTag.TagId;
                }

                await _unitOfWork.ProductTags.AssignProductTagAsync(productTag);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (System.Exception)
            {

                throw;
            }
        }

        public void AssignProductTagRange(List<ProductTagsRegistry> tags, int productId)
        {
            try
            {
                foreach (var tag in tags)
                {
                    var productTag = new ProductTag { ProductId = productId };
                    if (tag.TagId != 0)
                    {
                        _unitOfWork.ProductTags.Update(tag);
                        productTag.TagId = tag.TagId;
                    }
                    else
                    {
                        var newTag = _unitOfWork.ProductTags.Add(tag);
                        productTag.TagId = newTag.TagId;
                    }

                    _unitOfWork.ProductTags.AssignProductTag(productTag);
                    _unitOfWork.SaveChanges();
                }
            }
            catch (System.Exception)
            {

                throw;
            }
        }

        public async Task AssignProductTagRangeAsync(List<ProductTagsRegistry> tags, int productId)
        {
            try
            {
                foreach (var tag in tags)
                {
                    var productTag = new ProductTag { ProductId = productId };
                    if (tag.TagId != 0)
                    {
                        _unitOfWork.ProductTags.Update(tag);
                        productTag.TagId = tag.TagId;
                    }
                    else
                    {
                        var newTag = await _unitOfWork.ProductTags.AddAsync(tag);
                        await _unitOfWork.SaveChangesAsync();
                        productTag.TagId = newTag.TagId;
                    }

                    await _unitOfWork.ProductTags.AssignProductTagAsync(productTag);
                    await _unitOfWork.SaveChangesAsync();
                }
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public async Task UnAssignProductTagAsync(int productId, int tagId)
        {
            await _unitOfWork.ProductTags.UnAssignProductTagAsync(productId, tagId);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}