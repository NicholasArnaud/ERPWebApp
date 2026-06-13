using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Invoices;

namespace ERPWebApp.Data.Repositories
{
    public class EasyPostInvoiceRepository : Repository<EasyPostInvoices>, IEasyPostInvoiceRepository
    {
        public EasyPostInvoiceRepository(ApplicationDbContext context) : base(context)
        {

        }
    }
}
