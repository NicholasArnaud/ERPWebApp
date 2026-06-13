using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;
using ERPWebApp.Services.IServices;
using Microsoft.AspNetCore.Mvc;
namespace ERPWebApp.Services
{
    public class BarcodeScanService : Service<BarcodeScan>, IBarcodeScanService
    {
        IUnitOfWork _unitOfWork;
        public BarcodeScanService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public JsonResult GetBarcodeChartDetails()
        {
           return _unitOfWork.BarcodeScans.GetBarcodeChartDetails();
        }
    }
}
