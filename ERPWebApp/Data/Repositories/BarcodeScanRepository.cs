using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace ERPWebApp.Data.Repositories
{
    public class BarcodeScanRepository : Repository<BarcodeScan>, IBarcodeScanRepository
    {
        public BarcodeScanRepository(ApplicationDbContext context) : base(context)
        {
        }

        public JsonResult GetBarcodeChartDetails()
        {
            DateTime nowTime =
             TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));

            int[] embroideryPlot = new int[24];
            int[] engravingPlot = new int[24];
            int[] metalPlot = new int[24];
            int[] uvpPlot = new int[24];
            int[] unknownPlot = new int[24];

            var newList =
              (from b in _context.BarcodeScan
               where b.ModifyDate.Date == nowTime.Date
               from so in _context.ShipStationAwaitingOrder
               where b.ShipStationOrderId == so.SSOrderId
               from p in _context.Product
               where so.ItemSku.Contains(p.Sku)
               select (
                    new
                    {
                        ModifyDate = b.ModifyDate,
                        ScanCode = b.BarcodeScanCode,
                        SSid = b.ShipStationOrderId,
                        SKU = p.Sku,
                        IsEmbroidery = p.IsEmbroidery,
                        IsEngraving = p.IsEngraving,
                        IsMetal = p.IsMetal,
                        IsUV = p.IsUv

                    })).ToList();



            foreach (var item in newList)
            {
                if (item.IsEmbroidery)
                {
                    embroideryPlot[item.ModifyDate.Hour] += 1;
                }
                else if (item.IsEngraving)
                {
                    engravingPlot[item.ModifyDate.Hour] += 1;
                }
                else if (item.IsMetal)
                {
                    metalPlot[item.ModifyDate.Hour] += 1;
                }
                else if (item.IsUV)
                {
                    uvpPlot[item.ModifyDate.Hour] += 1;
                }
                else
                {
                    embroideryPlot[item.ModifyDate.Hour] += 1;
                }

            }

            var data = new
            {
                embroideryPlotArray = embroideryPlot,
                engravingPlotArray = engravingPlot,
                metalPlotArray = metalPlot,
                uvpPlotArray = uvpPlot,
                unknownPlotArray = unknownPlot
            };

            return new JsonResult(data);
        }
    }
}
