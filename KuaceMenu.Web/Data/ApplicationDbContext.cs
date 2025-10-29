using KuaceMenu.Web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KuaceMenu.Web.Data;

public class ApplicationDbContext : IdentityDbContext<AppUser>
{
	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
		: base(options)
	{
	}

	public DbSet<Tenant> Tenants => Set<Tenant>();
	public DbSet<MenuCategory> MenuCategories => Set<MenuCategory>();
	public DbSet<MenuItem> MenuItems => Set<MenuItem>();
	public DbSet<Payment> Payments => Set<Payment>();
	public DbSet<DomainConfig> DomainConfigs => Set<DomainConfig>();

	protected override void OnModelCreating(ModelBuilder builder)
	{
		base.OnModelCreating(builder);

		// Tenant
		builder.Entity<Tenant>(entity =>
		{
			entity.HasIndex(t => t.Slug).IsUnique();
			entity.Property(t => t.Name).HasMaxLength(200);
			entity.Property(t => t.Slug).HasMaxLength(100);
		});

		// MenuCategory
		// MenuCategory -> Tenant
		builder.Entity<MenuCategory>()
			.HasOne(c => c.Tenant)
			.WithMany(t => t.Categories)
			.HasForeignKey(c => c.TenantId)
			.OnDelete(DeleteBehavior.NoAction);

		// MenuItem -> MenuCategory
		builder.Entity<MenuItem>()
			.HasOne(i => i.MenuCategory)
			.WithMany(c => c.Items)
			.HasForeignKey(i => i.MenuCategoryId)
			.OnDelete(DeleteBehavior.Cascade);

		// MenuItem -> Tenant
		builder.Entity<MenuItem>()
			.HasOne(i => i.Tenant)
			.WithMany(t => t.MenuItems)
			.HasForeignKey(i => i.TenantId)
			.OnDelete(DeleteBehavior.NoAction);


		// Payment
		builder.Entity<Payment>(entity =>
		{
			entity.Property(p => p.Provider).HasMaxLength(50);
			entity.Property(p => p.Currency).HasMaxLength(10);
			entity.Property(p => p.Status).HasMaxLength(50);
		});

		// DomainConfig
		builder.Entity<DomainConfig>(entity =>
		{
			entity.Property(d => d.BaseDomain).HasMaxLength(200);
		});
	}
}
