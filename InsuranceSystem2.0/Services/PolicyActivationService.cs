using InsuranceSystem2._0.Data;
using InsuranceSystem2._0.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace InsuranceSystem2._0.Services.Implementations
{
    // Activates Scheduled policies once their start date has arrived.
    public class PolicyActivationService : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(15);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PolicyActivationService> _logger;

        public PolicyActivationService(
            IServiceScopeFactory scopeFactory,
            ILogger<PolicyActivationService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ActivateDuePoliciesAsync(stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Error while activating scheduled policies.");
                }

                try
                {
                    await Task.Delay(Interval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        private async Task ActivateDuePoliciesAsync(CancellationToken cancellationToken)
        {
            // BackgroundService is a singleton, so create a scope to use the DbContext
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var duePolicies = await context.Policies
                .Where(p =>
                    p.PolicyStatus == PolicyStatus.Scheduled &&
                    p.StartDate <= today &&
                    p.EndDate >= today)
                .ToListAsync(cancellationToken);

            if (duePolicies.Count == 0)
                return;

            foreach (var policy in duePolicies)
            {
                policy.PolicyStatus = PolicyStatus.Active;
                policy.UpdatedDate = DateTime.UtcNow;
            }

            await context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "{Count} scheduled policy(ies) activated.",
                duePolicies.Count);
        }
    }
}
