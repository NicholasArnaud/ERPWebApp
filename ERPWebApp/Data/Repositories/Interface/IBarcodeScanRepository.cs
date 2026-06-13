using ERPWebApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace ERPWebApp.Data.Repositories.Interface
{
    public interface IBarcodeScanRepository : IRepository<BarcodeScan>
    {
        JsonResult GetBarcodeChartDetails();
    }
}
