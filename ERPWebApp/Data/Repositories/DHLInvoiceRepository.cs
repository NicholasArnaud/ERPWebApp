using ERPWebApp.Models.Invoices;

namespace ERPWebApp.Data.Repositories
{
    public class DHLInvoiceRepository : Repository<DHLInvoices>, IDHLInvoiceRepository
    {
        public DHLInvoiceRepository(ApplicationDbContext context) : base(context)
        {

        }
    }
}
