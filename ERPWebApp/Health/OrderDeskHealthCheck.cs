using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ERPWebApp.Health;

public class OrderDeskHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
