using System;
using System.Collections.Generic;

namespace KuaceMenu.Web.Models;

public class Tenant
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? OwnerUserId { get; set; }
    public AppUser? Owner { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public SubscriptionStatus SubscriptionStatus { get; set; }
    public DateTime? SubscriptionStart { get; set; }
    public DateTime? SubscriptionEnd { get; set; }
    public DateTime? GraceUntil { get; set; }
    public string? LogoPath { get; set; }
    public string? ThemeColor { get; set; }
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }
    public bool IsPublicMenuEnabled { get; set; } = true;

    public ICollection<MenuCategory> Categories { get; set; } = new List<MenuCategory>();
    public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
