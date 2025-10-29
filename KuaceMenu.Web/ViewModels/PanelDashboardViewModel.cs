using System;
using System.Collections.Generic;
using KuaceMenu.Web.Models;

namespace KuaceMenu.Web.ViewModels;

public class PanelDashboardViewModel
{
    public Tenant Tenant { get; set; } = null!;
    public IReadOnlyList<Payment> Payments { get; set; } = Array.Empty<Payment>();
}
