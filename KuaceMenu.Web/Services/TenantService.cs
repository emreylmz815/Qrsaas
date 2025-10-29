using System;
using System.Threading.Tasks;
using KuaceMenu.Web.Data;
using KuaceMenu.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace KuaceMenu.Web.Services;

public interface ITenantService
{
    Task<Tenant?> GetTenantBySlugAsync(string slug);
    Task<Tenant> CreateTenantAsync(Tenant tenant);
    Task<bool> SlugExistsAsync(string slug);
    Task UpdateSubscriptionAsync(Tenant tenant, SubscriptionStatus status, DateTime start, DateTime end, DateTime graceUntil);
}

public class TenantService : ITenantService
{
    private readonly ApplicationDbContext _db;

    public TenantService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Tenant?> GetTenantBySlugAsync(string slug)
    {
        return await _db.Tenants.Include(t => t.Categories)
            .ThenInclude(c => c.Items)
            .FirstOrDefaultAsync(t => t.Slug == slug);
    }

    public async Task<Tenant> CreateTenantAsync(Tenant tenant)
    {
        _db.Tenants.Add(tenant);
        await _db.SaveChangesAsync();
        return tenant;
    }

    public Task<bool> SlugExistsAsync(string slug)
    {
        return _db.Tenants.AnyAsync(t => t.Slug == slug);
    }

    public async Task UpdateSubscriptionAsync(Tenant tenant, SubscriptionStatus status, DateTime start, DateTime end, DateTime graceUntil)
    {
        tenant.SubscriptionStatus = status;
        tenant.SubscriptionStart = start;
        tenant.SubscriptionEnd = end;
        tenant.GraceUntil = graceUntil;
        tenant.IsPublicMenuEnabled = status == SubscriptionStatus.Active;
        await _db.SaveChangesAsync();
    }
}
