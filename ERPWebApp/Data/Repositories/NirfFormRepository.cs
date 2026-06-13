using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.NirfForms;

namespace ERPWebApp.Data.Repositories
{
    public class NirfFormRepository : Repository<NirfForm>, INirfFormRepository
    {
        public NirfFormRepository(ApplicationDbContext context) : base(context)
        {
        }

        public IQueryable<NirfForm> GetAllNirfFormIdById(int nirfFormId)
        {
            return from x in _context.NirfForm
                   where x.NirfFormId == nirfFormId
                   join y in _context.NirfInventory on x.NirfFormId equals y.NirfFormId
                   join z in _context.NirfParameters on x.NirfFormId equals z.NirfFormId
                   join a in _context.NirfPackaging on x.NirfFormId equals a.NirfFormId
                   join b in _context.NirfForecasting on x.NirfFormId equals b.NirfFormId
                   join c in _context.NirfShipping on x.NirfFormId equals c.NirfFormId
                   join d in _context.NirfVendorMapping on x.NirfFormId equals d.NirfFormId
                   select new NirfForm { NirfFormId = x.NirfFormId };

        }
    }
}