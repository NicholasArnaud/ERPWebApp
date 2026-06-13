using ERPWebApp.Models.Inventory;

namespace ERPWebApp.Services.IServices
{
    public interface IBundleService: IService<Bundle>
    {
        Task<Bundle> GetBundleWithItemsAsync(int id);
    }
}
