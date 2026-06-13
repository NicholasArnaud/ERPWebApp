using ERPWebApp.Models.NirfForms;
namespace ERPWebApp.Services.IServices
{
    public interface INirfFormService : IService<NirfForm>
    {
        IQueryable<NirfForm> GetAllNirfFormIdById(int nirfFormId);
    }
}