using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using KuaceMenu.Web.Filters;
using KuaceMenu.Web.Models;
using KuaceMenu.Web.Services;
using KuaceMenu.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace KuaceMenu.Web.Controllers;

[Authorize]
[ServiceFilter(typeof(TenantAuthorizationFilter))]
[Route("panel")]
public class PanelController : Controller
{
    private readonly ITenantContext _tenantContext;
    private readonly IMenuService _menuService;
    private readonly IPaymentService _paymentService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<PanelController> _logger;

    public PanelController(
        ITenantContext tenantContext,
        IMenuService menuService,
        IPaymentService paymentService,
        IWebHostEnvironment environment,
        ILogger<PanelController> logger)
    {
        _tenantContext = tenantContext;
        _menuService = menuService;
        _paymentService = paymentService;
        _environment = environment;
        _logger = logger;
    }

    [HttpGet("")]
    [HttpGet("index")]
    public async Task<IActionResult> Index()
    {
        var tenant = _tenantContext.Tenant!;
        var payments = await _paymentService.GetPaymentsAsync(tenant.Id);
        var vm = new PanelDashboardViewModel
        {
            Tenant = tenant,
            Payments = payments
        };
        return View(vm);
    }

    [HttpGet("categories")]
    public async Task<IActionResult> Categories()
    {
        var tenant = _tenantContext.Tenant!;
        var categories = await _menuService.GetCategoriesAsync(tenant.Id);
        var vm = new CategoryListViewModel
        {
            Tenant = tenant,
            Categories = categories,
            CategoryInput = new CategoryInputModel()
        };
        return View(vm);
    }

    [HttpPost("categories")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(CategoryInputModel model)
    {
        var tenant = _tenantContext.Tenant!;
        if (!ModelState.IsValid)
        {
            return await Categories();
        }

        var category = new MenuCategory
        {
            Name = model.Name,
            DisplayOrder = model.DisplayOrder,
            TenantId = tenant.Id
        };
        await _menuService.AddCategoryAsync(category);
        TempData["Success"] = "Kategori eklendi";
        return RedirectToAction(nameof(Categories));
    }

    [HttpPost("categories/delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var tenant = _tenantContext.Tenant!;
        var category = await _menuService.FindCategoryAsync(id, tenant.Id);
        if (category is not null)
        {
            await _menuService.DeleteCategoryAsync(category);
            TempData["Success"] = "Kategori silindi";
        }
        return RedirectToAction(nameof(Categories));
    }

    [HttpGet("items")]
    public async Task<IActionResult> Items()
    {
        var tenant = _tenantContext.Tenant!;
        var categories = await _menuService.GetCategoriesAsync(tenant.Id);
        var vm = new MenuItemListViewModel
        {
            Tenant = tenant,
            Categories = categories,
            ItemInput = new MenuItemInputModel()
        };
        return View(vm);
    }

    [HttpPost("items")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateItem(MenuItemInputModel model)
    {
        var tenant = _tenantContext.Tenant!;
        if (!ModelState.IsValid)
        {
            return await Items();
        }

        string? photoPath = null;
        if (model.Photo is not null && model.Photo.Length > 0)
        {
            var extension = Path.GetExtension(model.Photo.FileName).ToLowerInvariant();
            var allowed = new[] { ".jpg", ".jpeg", ".png" };
            if (!allowed.Contains(extension))
            {
                ModelState.AddModelError("Photo", "Sadece jpg/png yükleyebilirsiniz");
                return await Items();
            }
            if (model.Photo.Length > 2 * 1024 * 1024)
            {
                ModelState.AddModelError("Photo", "Dosya boyutu 2MB'den küçük olmalı");
                return await Items();
            }

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var path = Path.Combine(uploadsFolder, fileName);
            using var stream = System.IO.File.Create(path);
            await model.Photo.CopyToAsync(stream);
            photoPath = $"/uploads/{fileName}";
        }

        var item = new MenuItem
        {
            TenantId = tenant.Id,
            MenuCategoryId = model.MenuCategoryId,
            Name = model.Name,
            Description = model.Description,
            Price = model.Price,
            PhotoPath = photoPath,
            IsActive = model.IsActive
        };
        await _menuService.AddItemAsync(item);
        TempData["Success"] = "Ürün eklendi";
        return RedirectToAction(nameof(Items));
    }

    [HttpPost("items/delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteItem(int id)
    {
        var tenant = _tenantContext.Tenant!;
        var item = await _menuService.FindItemAsync(id, tenant.Id);
        if (item is not null)
        {
            await _menuService.DeleteItemAsync(item);
            TempData["Success"] = "Ürün silindi";
        }
        return RedirectToAction(nameof(Items));
    }
}
