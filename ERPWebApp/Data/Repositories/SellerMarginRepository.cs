using ERPWebApp.Models.Sellers;
using ERPWebApp.Data.Repositories.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;
namespace ERPWebApp.Data.Repositories
{

    public class SellerMarginRepository : Repository<SellerMargins>, ISellerMarginRepository
    {
        private readonly ApplicationDbContext _context;

        public SellerMarginRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<SellerMargins>> GetSellerMarginsAsync()
        {
            return await _context.SellerMargins.ToListAsync();
        }

        public async Task<List<SellerMargins>> GetSellerMarginsByDateRangeAsync(int? storeId, DateTime startDate, DateTime endDate)
        {
            var conn = _context.Database.GetDbConnection();
            conn.Open();
            var sellerMargins = new List<SellerMargins>();

            try
            {
                using var command = conn.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "GetSellerInvoiceMarginByStoreIdAndDateRange";
                List<SqlParameter> param = new()
                {
                    new SqlParameter("@StoreId", SqlDbType.Int) { Value = storeId },
                    new SqlParameter("@StartDate", SqlDbType.DateTime) { Value = startDate.ToString() },
                    new SqlParameter("@EndDate", SqlDbType.DateTime) { Value = endDate.ToString() }
                };
                command.Parameters.AddRange(param.ToArray());
                DbDataReader reader = await command.ExecuteReaderAsync();

                if (reader.HasRows)
                {
                    while (await reader.ReadAsync())
                    {
                        try
                        {
                            // CustomField3 Designer Information  
                            SellerMargins sellerinvoiceMargins = new()
                            {
                                OrderNumber = reader.GetString(0),
                                ShipDate = reader.GetDateTime(1),
                                StoreName = reader.GetString(2),
                                ServiceCode = reader.GetString(3),
                                TrackingNumber = reader.GetString(4),
                                StoreItemsCost = reader.GetDecimal(5),
                                CustomerItemsCost = reader.GetDecimal(6),
                                ShippingCost = reader.GetDecimal(7),
                                ShipmentCost = reader.GetDecimal(8),
                                StoreCostWithEtsy = reader.GetDecimal(9),
                                StoreCostDiffSubfulfillmentAndShipping = reader.GetDecimal(10)
                            };
                            sellerinvoiceMargins.StringShipDate = sellerinvoiceMargins.ShipDate.ToShortDateString();
                            sellerinvoiceMargins.StringShipDate = sellerinvoiceMargins.StringShipDate.Replace("/", "");
                            sellerMargins.Add(sellerinvoiceMargins);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("An issue has arisen:" + ex.Message);
                        }
                    }
                }
                reader.Close();
            }
            catch
            {
                return sellerMargins;
            }
            finally
            {
                conn.Close();
            }
            return sellerMargins;
        }
    }
}
