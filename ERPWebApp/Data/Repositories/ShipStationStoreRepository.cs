using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;
using ERPWebApp.Models.Mappings;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Data.Common;

namespace ERPWebApp.Data.Repositories
{
    public class ShipStationStoreRepository : Repository<ShipStationStore>, IShipStationStoreRepository
    {
        public ShipStationStoreRepository(ApplicationDbContext context) : base(context)
        {
        }

        public JObject GetShipStationStorePieChartsData(DateTime date)
        {
            var conn = _context.Database.GetDbConnection();
            try
            {
                var data = new
                {
                    electroplatingPlot = new List<int>(),
                    electroplatingSkus = new List<String>(),
                    embroideryPlot = new List<int>(),
                    embroiderySkus = new List<String>(),
                    engravingPlot = new List<int>(),
                    engravingSkus = new List<String>(),
                    metalPlot = new List<int>(),
                    metalSkus = new List<String>(),
                    uvpPlot = new List<int>(),
                    uvpSkus = new List<String>(),
                    unknownPlot = new List<int>(),
                    unknownSkus = new List<String>()
                };

                conn.Open();
                using var command = conn.CreateCommand();

                command.CommandText =
                    "SELECT SUM(oi.quantity) as Total, " +
                    "COALESCE(p.Sku, oi.sku) as SKU, " +
                    "COALESCE(d.DepartmentName, 'unknown') as DepartmentName " +
                    "FROM Orders o " +
                    "INNER JOIN OrderItem oi ON o.ERPOrderId = oi.ERPOrderId " +
                    "LEFT JOIN Product p ON oi.ERPProductId = p.ProductId " +
                    "LEFT JOIN DepartmentProduct dp ON p.ProductId = dp.ProductsProductId " +
                    "LEFT JOIN Department d ON dp.DepartmentsDepartmentId = d.DepartmentId " +
                    "WHERE CAST(o.shipDate as date) = CAST(GETDATE() as date) AND o.orderStatus = 2 " +
                    "AND NOT oi.sku = '' " +
                    "AND ((oi.sku LIKE '%uvp%' AND dp.DepartmentsDepartmentId = 3) OR (oi.sku NOT LIKE '%uvp%' AND dp.DepartmentsDepartmentId != 3)) " +
                    "GROUP BY COALESCE(d.DepartmentName, 'unknown'), COALESCE(p.Sku, oi.sku) " +
                    "ORDER BY Total DESC; ";

                DbDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var SKU = "";
                        var dept = "";

                        var Count = reader.GetInt32(0);

                        if (reader.GetString(1) != null)
                        {
                            SKU = reader.GetString(1);
                        }
                        else
                        {
                            SKU = "UNKNOWN";
                        }

                        if (reader.GetString(2) != null)
                        {
                            dept = reader.GetString(2);
                        }
                        else
                        {
                            dept = "UNKNOWN";
                        }


                        if (dept.Contains("Engraving"))
                        {
                            data.engravingPlot.Add(Count);
                            data.engravingSkus.Add(SKU);
                        }
                        else if (dept.Contains("UVP"))
                        {
                            data.uvpPlot.Add(Count);
                            data.uvpSkus.Add(SKU);
                        }
                        else if (dept.Contains("Metal"))
                        {
                            data.metalPlot.Add(Count);
                            data.metalSkus.Add(SKU);
                        }
                        else if (dept.Contains("Embroidery"))
                        {
                            data.embroideryPlot.Add(Count);
                            data.embroiderySkus.Add(SKU);
                        }
                        else if (dept.Contains("Electroplating"))
                        {
                            data.electroplatingPlot.Add(Count);
                            data.electroplatingSkus.Add(SKU);
                        }
                        else if (dept.Contains("unknown"))
                        {
                            data.unknownPlot.Add(Count);
                            data.unknownSkus.Add(SKU);
                        }
                    }
                }

                reader.Close();


                return JObject.FromObject(data);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                conn.Close();
            }
        }

        public async Task<string> GetStoreNameById(long storeId)
        {
            return await _context.ShipStationStore.Where(x => x.StoreId == storeId)
                                .Select(x => x.StoreName).FirstOrDefaultAsync();
        }

        public async Task<ShipStationStore> GetShipStationStoreByEmailAsync(string email)
        {
            return await _context.ShipStationStore.FirstOrDefaultAsync(s => s.Email == email || s.PublicEmail == email);
        }

        public async Task<List<ShipStationStore>> GetAllOrderedByNameAsync()
        {
            return await _context.ShipStationStore.OrderBy(s => s.StoreName).ToListAsync();
        }

        public async Task<ShipStationStore> GetFirstOrderedByNameAsync()
        {
            return await _context.ShipStationStore.OrderBy(s => s.StoreName).FirstOrDefaultAsync();
        }

        public async Task AddStoreFileAsync(ShipStationStoreFile file)
        {
            await _context.ShipStationStoreFiles.AddAsync(file);
        }

        public async Task DeleteStoreFileAsync(int storeFileId)
        {
            var file = await _context.ShipStationStoreFiles.FindAsync(storeFileId);
            if (file != null)
                _context.ShipStationStoreFiles.Remove(file);
        }

        public async Task<ShipStationStoreFile> GetStoreFileAsync(int storeFileId)
        {
            return await _context.ShipStationStoreFiles
            .Include(x => x.Files)
            .FirstOrDefaultAsync(x => x.StoreFileId == storeFileId);
        }
    }
}