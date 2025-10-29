using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using KuaceMenu.Web.Data;
using KuaceMenu.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace KuaceMenu.Web.Services;

public record PaymentSession(string Token, string HtmlContent);

public interface IPaymentService
{
    Task<PaymentSession> CreateCheckoutSessionAsync(Tenant tenant, AppUser user, HttpRequest request);
    Task<Payment?> CompletePaymentAsync(int tenantId, string token, bool success, string? rawJson = null);
    Task<IReadOnlyList<Payment>> GetPaymentsAsync(int tenantId);
}

public class PaymentService : IPaymentService
{
    private readonly ApplicationDbContext _db;
    private readonly IOptions<IyzicoSettings> _settings;
    private readonly IClock _clock;

    public PaymentService(ApplicationDbContext db, IOptions<IyzicoSettings> settings, IClock clock)
    {
        _db = db;
        _settings = settings;
        _clock = clock;
    }

    public async Task<PaymentSession> CreateCheckoutSessionAsync(Tenant tenant, AppUser user, HttpRequest request)
    {
        var settings = _settings.Value;
        var callback = string.IsNullOrWhiteSpace(settings.CallbackUrl)
            ? new Uri(new Uri($"{request.Scheme}://{request.Host}"), "/payment/callback").ToString()
            : settings.CallbackUrl;

        var token = Guid.NewGuid().ToString("N");

		var html = $$"""
<div class="alert alert-info">
    Iyzico sandbox anahtarlarını appsettings.json dosyasına ekleyin. Token: {{token}}
</div>
""";

		var meta = new
        {
            tenantId = tenant.Id,
            token,
            amount = settings.AnnualPrice,
            currency = "TRY",
            callback
        };

        var rawJson = JsonSerializer.Serialize(meta);
        await _db.Payments.AddAsync(new Payment
        {
            TenantId = tenant.Id,
            Amount = settings.AnnualPrice,
            Currency = "TRY",
            PaidAt = _clock.UtcNow,
            PeriodStart = _clock.UtcNow,
            PeriodEnd = _clock.UtcNow,
            Provider = "Iyzico",
            ProviderRef = token,
            Status = "Pending",
            RawJson = rawJson
        });
        await _db.SaveChangesAsync();

        return new PaymentSession(token, html);
    }

    public async Task<Payment?> CompletePaymentAsync(int tenantId, string token, bool success, string? rawJson = null)
    {
        var payment = await _db.Payments.FirstOrDefaultAsync(p => p.ProviderRef == token && p.TenantId == tenantId);
        if (payment is null)
        {
            return null;
        }

        payment.Status = success ? "Succeeded" : "Failed";
        payment.RawJson = rawJson ?? payment.RawJson;
        payment.PaidAt = _clock.UtcNow;

        if (success)
        {
            var start = _clock.UtcNow;
            var end = start.AddYears(1);
            payment.PeriodStart = start;
            payment.PeriodEnd = end;

            var tenant = await _db.Tenants.FindAsync(tenantId);
            if (tenant is not null)
            {
                tenant.SubscriptionStatus = SubscriptionStatus.Active;
                tenant.SubscriptionStart = start;
                tenant.SubscriptionEnd = end;
                tenant.GraceUntil = end.AddDays(7);
                tenant.IsPublicMenuEnabled = true;
            }
        }
        else
        {
            payment.PeriodStart = _clock.UtcNow;
            payment.PeriodEnd = _clock.UtcNow;
        }

        await _db.SaveChangesAsync();
        return payment;
    }

    public async Task<IReadOnlyList<Payment>> GetPaymentsAsync(int tenantId)
    {
        return await _db.Payments
            .Where(p => p.TenantId == tenantId)
            .OrderByDescending(p => p.PaidAt)
            .ToListAsync();
    }
}
