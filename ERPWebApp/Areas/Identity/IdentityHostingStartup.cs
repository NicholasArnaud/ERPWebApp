[assembly: HostingStartup(typeof(ERPWebApp.Areas.Identity.IdentityHostingStartup))]
namespace ERPWebApp.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
            });
        }
    }
}