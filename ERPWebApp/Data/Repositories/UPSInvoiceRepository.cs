using ERPWebApp.Models.Invoices;

namespace ERPWebApp.Data.Repositories
{
    public class UPSInvoiceRepository : Repository<UPSInvoices>, IUPSInvoiceRepository
    {
        public UPSInvoiceRepository(ApplicationDbContext context) : base(context)
        {

        }
    }
}
