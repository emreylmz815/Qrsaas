namespace KuaceMenu.Web.Models;

public class MenuItem
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public int MenuCategoryId { get; set; }
    public MenuCategory? MenuCategory { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? PhotoPath { get; set; }
    public bool IsActive { get; set; } = true;
}
