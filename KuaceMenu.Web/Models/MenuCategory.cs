using System.Collections.Generic;

namespace KuaceMenu.Web.Models;

public class MenuCategory
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public ICollection<MenuItem> Items { get; set; } = new List<MenuItem>();
}
