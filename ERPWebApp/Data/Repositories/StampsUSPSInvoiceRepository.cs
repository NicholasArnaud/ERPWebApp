using ERPWebApp.Models.Invoices;

namespace ERPWebApp.Data.Repositories
{
    public class StampsUSPSInvoiceRepository : Repository<StampsUSPSInvoices>, IStampsUSPSInvoiceRepository
    {
        public StampsUSPSInvoiceRepository(ApplicationDbContext context) : base(context)
        {

        }
    }
}
