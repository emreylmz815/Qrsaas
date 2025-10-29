using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KuaceMenu.Web.Data;
using KuaceMenu.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace KuaceMenu.Web.Services;

public interface IMenuService
{
    Task<IReadOnlyList<MenuCategory>> GetCategoriesAsync(int tenantId);
    Task<MenuCategory> AddCategoryAsync(MenuCategory category);
    Task<MenuItem> AddItemAsync(MenuItem item);
    Task<MenuCategory?> FindCategoryAsync(int id, int tenantId);
    Task<MenuItem?> FindItemAsync(int id, int tenantId);
    Task DeleteCategoryAsync(MenuCategory category);
    Task DeleteItemAsync(MenuItem item);
}

public class MenuService : IMenuService
{
    private readonly ApplicationDbContext _db;

    public MenuService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<MenuCategory>> GetCategoriesAsync(int tenantId)
    {
        var categories = await _db.MenuCategories
            .Where(c => c.TenantId == tenantId)
            .Include(c => c.Items)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();

        foreach (var category in categories)
        {
            category.Items = category.Items.OrderBy(i => i.Name).ToList();
        }

        return categories;
    }

    public async Task<MenuCategory> AddCategoryAsync(MenuCategory category)
    {
        _db.MenuCategories.Add(category);
        await _db.SaveChangesAsync();
        return category;
    }

    public async Task<MenuItem> AddItemAsync(MenuItem item)
    {
        _db.MenuItems.Add(item);
        await _db.SaveChangesAsync();
        return item;
    }

    public Task<MenuCategory?> FindCategoryAsync(int id, int tenantId)
    {
        return _db.MenuCategories.FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);
    }

    public Task<MenuItem?> FindItemAsync(int id, int tenantId)
    {
        return _db.MenuItems.FirstOrDefaultAsync(i => i.Id == id && i.TenantId == tenantId);
    }

    public async Task DeleteCategoryAsync(MenuCategory category)
    {
        _db.MenuCategories.Remove(category);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteItemAsync(MenuItem item)
    {
        _db.MenuItems.Remove(item);
        await _db.SaveChangesAsync();
    }
}
