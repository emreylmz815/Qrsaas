using KuaceMenu.Web.Models;

namespace KuaceMenu.Web.Services;

public class TenantContext : ITenantContext
{
    public Tenant? Tenant { get; set; }
    public string? Slug { get; set; }
}
