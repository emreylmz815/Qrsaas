using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace KuaceMenu.Web.Models;

public class AppUser : IdentityUser
{
    public string? FullName { get; set; }

    public ICollection<Tenant> OwnedTenants { get; set; } = new List<Tenant>();
}
