using System;
using System.Collections.Generic;
using KuaceMenu.Web.Models;

namespace KuaceMenu.Web.ViewModels;

public class PublicMenuViewModel
{
    public Tenant Tenant { get; set; } = null!;
    public IReadOnlyList<MenuCategory> Categories { get; set; } = Array.Empty<MenuCategory>();
    public string? Lang { get; set; }
}
