using KuaceMenu.Web.Models;

namespace KuaceMenu.Web.Services;

public interface ITenantContext
{
    Tenant? Tenant { get; set; }
    string? Slug { get; set; }
}
