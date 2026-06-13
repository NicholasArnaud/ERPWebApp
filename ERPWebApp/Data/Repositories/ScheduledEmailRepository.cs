using System.Data;
using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;
using Microsoft.EntityFrameworkCore;
namespace ERPWebApp.Data.Repositories
{
    public class ScheduledEmailRepository : Repository<EmailAlert>, IScheduledEmailRepository
    {
        private readonly ApplicationDbContext _context;

        public ScheduledEmailRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<EmailAlert>> GetAllEmailAlertsAsync()
        {
            return await _context.EmailAlerts.ToListAsync();
        }

        public async Task<List<string>> GetRecipientsForEmailAlertAsync(int emailAlertId)
        {
            return await _context.UserEmailAlertMapping
                .Where(mapping => mapping.EmailAlertId == emailAlertId)
                .Select(mapping => mapping.UserEmail)
                .ToListAsync();
        }
    }
}
