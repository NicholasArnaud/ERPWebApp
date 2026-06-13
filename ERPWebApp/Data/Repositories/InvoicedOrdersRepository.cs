using ERPWebApp.Models.Invoices;

namespace ERPWebApp.Data.Repositories
{
    public class InvoicedOrdersRepository : Repository<InvoicedOrders>, IInvoicedOrdersRepository
    {
        public InvoicedOrdersRepository(ApplicationDbContext context) : base(context)
        {

        }
    }
}
