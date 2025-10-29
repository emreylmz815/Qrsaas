using System.Threading.Tasks;
using KuaceMenu.Web.Data;
using KuaceMenu.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KuaceMenu.Web.Services;

public class SubscriptionCheckJob
{
    private readonly ApplicationDbContext _db;
    private readonly IClock _clock;
    private readonly ILogger<SubscriptionCheckJob> _logger;

    public SubscriptionCheckJob(ApplicationDbContext db, IClock clock, ILogger<SubscriptionCheckJob> logger)
    {
        _db = db;
        _clock = clock;
        _logger = logger;
    }

    public async Task RunAsync()
    {
        var now = _clock.UtcNow;
        _logger.LogInformation("Subscription kontrolü {Now}", now);

        var tenants = await _db.Tenants.ToListAsync();
        foreach (var tenant in tenants)
        {
            if (tenant.SubscriptionStatus == SubscriptionStatus.Active && tenant.SubscriptionEnd.HasValue && tenant.SubscriptionEnd < now)
            {
                tenant.SubscriptionStatus = SubscriptionStatus.PastDue;
            }

            if (tenant.GraceUntil.HasValue && tenant.GraceUntil < now)
            {
                tenant.SubscriptionStatus = SubscriptionStatus.Expired;
                tenant.IsPublicMenuEnabled = false;
            }
        }

        await _db.SaveChangesAsync();
    }
}
