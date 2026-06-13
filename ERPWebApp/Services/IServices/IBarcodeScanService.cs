using ERPWebApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace ERPWebApp.Services.IServices
{
    public interface IBarcodeScanService : IService<BarcodeScan>
    {
        JsonResult GetBarcodeChartDetails();
    }
}
