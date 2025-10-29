using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KuaceMenu.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace KuaceMenu.Web.Data;

public static class SeedData
{
    public static async Task EnsureSeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var scopedProvider = scope.ServiceProvider;

        var context = scopedProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        var userManager = scopedProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scopedProvider.GetRequiredService<RoleManager<IdentityRole>>();

        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        var adminEmail = "admin@kuacemenu.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser is null)
        {
            adminUser = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = "Platform Admin"
            };

            var createResult = await userManager.CreateAsync(adminUser, "Admin*123");
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        if (!await context.Tenants.AnyAsync(t => t.Slug == "demo"))
        {
            var demoTenant = new Tenant
            {
                Name = "Demo Restoran",
                Slug = "demo",
                OwnerUserId = adminUser?.Id,
                CreatedAt = DateTime.UtcNow,
                SubscriptionStatus = SubscriptionStatus.Active,
                SubscriptionStart = DateTime.UtcNow,
                SubscriptionEnd = DateTime.UtcNow.AddDays(30),
                GraceUntil = DateTime.UtcNow.AddDays(37),
                ContactPhone = "+90 555 000 0000",
                Address = "Demo Mah. 123. Sok. No:4 İstanbul",
                IsPublicMenuEnabled = true
            };
            context.Tenants.Add(demoTenant);
            await context.SaveChangesAsync();

            var starters = new MenuCategory
            {
                Name = "Başlangıçlar",
                TenantId = demoTenant.Id,
                DisplayOrder = 1
            };
            var mains = new MenuCategory
            {
                Name = "Ana Yemekler",
                TenantId = demoTenant.Id,
                DisplayOrder = 2
            };
            context.MenuCategories.AddRange(starters, mains);
            await context.SaveChangesAsync();

            var demoItems = new List<MenuItem>
            {
                new()
                {
                    Name = "Mercimek Çorbası",
                    Description = "Geleneksel mercimek çorbası",
                    Price = 60,
                    TenantId = demoTenant.Id,
                    MenuCategoryId = starters.Id,
                    IsActive = true
                },
                new()
                {
                    Name = "Humus",
                    Description = "Nohut ezmesi",
                    Price = 75,
                    TenantId = demoTenant.Id,
                    MenuCategoryId = starters.Id,
                    IsActive = true
                },
                new()
                {
                    Name = "Izgara Tavuk",
                    Description = "Izgara sebzeler ile",
                    Price = 160,
                    TenantId = demoTenant.Id,
                    MenuCategoryId = mains.Id,
                    IsActive = true
                },
                new()
                {
                    Name = "Et Döner",
                    Description = "Odun ateşinde",
                    Price = 190,
                    TenantId = demoTenant.Id,
                    MenuCategoryId = mains.Id,
                    IsActive = true
                },
                new()
                {
                    Name = "Vegan Bowl",
                    Description = "Sebzeli kinoa",
                    Price = 145,
                    TenantId = demoTenant.Id,
                    MenuCategoryId = mains.Id,
                    IsActive = true
                }
            };
            context.MenuItems.AddRange(demoItems);
            await context.SaveChangesAsync();
        }

        if (!await context.DomainConfigs.AnyAsync())
        {
            context.DomainConfigs.Add(new DomainConfig
            {
                BaseDomain = "kuacemenu.com",
                WildcardEnabled = true
            });
            await context.SaveChangesAsync();
        }
    }
}
