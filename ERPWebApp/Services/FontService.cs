using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.NirfForms;
using ERPWebApp.Services.IServices;

namespace ERPWebApp.Services
{
    public class FontService : Service<Fonts>, IFontService
    {
        IUnitOfWork _unitOfWork;

        public FontService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
    }
}