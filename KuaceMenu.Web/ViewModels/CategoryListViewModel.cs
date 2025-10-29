using System;
using System.Collections.Generic;
using KuaceMenu.Web.Models;

namespace KuaceMenu.Web.ViewModels;

public class CategoryListViewModel
{
    public Tenant Tenant { get; set; } = null!;
    public IReadOnlyList<MenuCategory> Categories { get; set; } = Array.Empty<MenuCategory>();
    public CategoryInputModel CategoryInput { get; set; } = new();
}
