using ERPWebApp.Data.DTOModels.ShippingScanout;
using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;
using ERPWebApp.Models.Orders;
using Microsoft.EntityFrameworkCore;

namespace ERPWebApp.Data.Repositories;

public class ShippingScanoutRepository(ApplicationDbContext context)
    : Repository<ShippingScanout>(context), IShippingScanoutRepository
{
    public async Task<IReadOnlyCollection<ShipmentsCountByCarrier>> GetOpenShipmentsCountByCarrierAsync()
    {
        var query = (
            from a in _context.ShippingScanout
            join b in _context.OrderShipments on a.OrderShipmentId equals b.OrderShipmentId
            where a.WebhookBatchId == null && a.CreateDate > DateTime.Today
            group a by b.carrierCode
            into g
            select new ShipmentsCountByCarrier(g.Key, g.Count())
        );
        return await query.AsNoTracking().ToListAsync();
    }

    public async Task<(List<string>, int)> GetScannedUspsTrackingNumbersAsync((string name, string state, string postalCode) shipFrom)
    {
        shipFrom.name = shipFrom.name.ToLower().Trim();
        shipFrom.state = shipFrom.state.ToLower().Trim();
        shipFrom.postalCode = shipFrom.postalCode.ToLower();
        
        var query = (
            from a in _context.ShippingScanout
            join b in _context.OrderShipments on a.OrderShipmentId equals b.OrderShipmentId
            join c in _context.OrderFulfillments on b.ERPOrderId equals c.ERPOrderId //Join both Shipment and Fulfillment to ensure it only includes orders shipped via ERP
            where a.WebhookBatchId == null
                  && (b.carrierCode.ToLower().Contains("usps") || b.carrierCode.ToLower().Contains("stamps"))
                  && EF.Functions.DateDiffDay(a.CreateDate, DateTime.UtcNow) == 0
                  && b.shipFrom.name.ToLower().Trim() == shipFrom.name
                  && b.shipFrom.state.ToLower().Trim() == shipFrom.state
                  && b.shipFrom.postalCode.ToLower() == shipFrom.postalCode
            select new { b.trackingNumber, a.OrderShipmentId}
        );
        
        var result = await query.AsNoTracking().ToListAsync();
        var trackingNumbers = result.Select(a => a.trackingNumber).ToHashSet().ToList();
        var shipmentNumbers = result.Select(a => a.OrderShipmentId).Distinct().Count();
        
        return (trackingNumbers, shipmentNumbers);
    }

    public async Task<List<(string CarrierCode, int ScannedTotal)>> GetDaysScanoutsByCarrier()
    {
        var results = await _context.ShippingScanout
            .GroupJoin(_context.OrderShipments,
            ss => ss.OrderShipmentId,
            os => os.OrderShipmentId,
            (ss, osGroup) => new { ss, osGroup })
            .SelectMany(x => x.osGroup.DefaultIfEmpty(),
                (x, os) => new { x.ss, os })
            .GroupJoin(_context.OrderFulfillments,
                x => x.ss.OrderFulfillmentId,
                oof => oof.OrderFulfillmentId,
                (x, oofGroup) => new { x, oofGroup })
            .SelectMany(y => y.oofGroup.DefaultIfEmpty(),
                (y, oof) => new { y.x.ss, y.x.os, oof })
            .Join(_context.Orders,
                z => (z.oof.ERPOrderId == 0) ? z.oof.ERPOrderId : z.os.ERPOrderId,
                o => o.ERPOrderId,
                (z, o) => new { z.ss, z.os, z.oof, o })
            .Where(c => c.ss.CreateDate > DateTime.Today)
            .GroupBy(c => c.oof.carrierCode ?? c.os.carrierCode)
            .AsNoTracking()
            .Select(grouped => new ValueTuple<string, int>(
                grouped.Key,
                grouped.Count()
            )).ToListAsync();

        return results;
    }
}