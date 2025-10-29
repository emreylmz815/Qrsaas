using System.Threading.Tasks;
using KuaceMenu.Web.Services;
using KuaceMenu.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace KuaceMenu.Web.Controllers;

[Route("{lang:regex(^[a-zA-Z]{2}$)?}")]
public class MenuController : Controller
{
    private readonly ITenantContext _tenantContext;
    private readonly IMenuService _menuService;

    public MenuController(ITenantContext tenantContext, IMenuService menuService)
    {
        _tenantContext = tenantContext;
        _menuService = menuService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? lang)
    {
        var tenant = _tenantContext.Tenant;
        if (tenant is null)
        {
            return RedirectToAction("Index", "Home");
        }

        var categories = await _menuService.GetCategoriesAsync(tenant.Id);
        var vm = new PublicMenuViewModel
        {
            Tenant = tenant,
            Categories = categories,
            Lang = lang
        };
        return View("Public", vm);
    }
}
