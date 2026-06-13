using ERPWebApp.Models.NirfForms;
namespace ERPWebApp.Data.Repositories.Interface
{
    public interface INirfFormRepository : IRepository<NirfForm>
    {
        IQueryable<NirfForm> GetAllNirfFormIdById(int nirfFormId);
    }
}