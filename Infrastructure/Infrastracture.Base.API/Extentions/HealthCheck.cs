namespace Infrastracture.Base.API.Extentions
{
    using Microsoft.Extensions.Diagnostics.HealthChecks;

    public class SelfHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            // Return a healthy status with a custom description
            return Task.FromResult(HealthCheckResult.Healthy("Fulfilment API is operating normally."));
        }
    }

}
