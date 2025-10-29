using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using KuaceMenu.Web.Models;
using KuaceMenu.Web.Services;
using KuaceMenu.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace KuaceMenu.Web.Controllers;

[Authorize]
public class TenantsController : Controller
{
    private readonly ITenantService _tenantService;
    private readonly UserManager<AppUser> _userManager;
    private readonly IWebHostEnvironment _environment;

    public TenantsController(ITenantService tenantService, UserManager<AppUser> userManager, IWebHostEnvironment environment)
    {
        _tenantService = tenantService;
        _userManager = userManager;
        _environment = environment;
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new TenantCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TenantCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (await _tenantService.SlugExistsAsync(model.Slug))
        {
            ModelState.AddModelError(nameof(model.Slug), "Bu alt alan adı kullanımda");
            return View(model);
        }

        string? logoPath = null;
        if (model.Logo is not null && model.Logo.Length > 0)
        {
            var extension = Path.GetExtension(model.Logo.FileName).ToLowerInvariant();
            var allowed = new[] { ".jpg", ".jpeg", ".png" };
            if (!allowed.Contains(extension))
            {
                ModelState.AddModelError(nameof(model.Logo), "Sadece jpg/png yükleyebilirsiniz");
                return View(model);
            }

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var path = Path.Combine(uploadsFolder, fileName);
            using var stream = System.IO.File.Create(path);
            await model.Logo.CopyToAsync(stream);
            logoPath = $"/uploads/{fileName}";
        }

        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        var tenant = new Tenant
        {
            Name = model.Name,
            Slug = model.Slug.ToLowerInvariant(),
            ContactPhone = model.ContactPhone,
            Address = model.Address,
            LogoPath = logoPath,
            OwnerUserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            SubscriptionStatus = SubscriptionStatus.Expired,
            IsPublicMenuEnabled = false
        };

        await _tenantService.CreateTenantAsync(tenant);

        return RedirectToAction("Checkout", "Payments", new { tenant = tenant.Slug });
    }
}
