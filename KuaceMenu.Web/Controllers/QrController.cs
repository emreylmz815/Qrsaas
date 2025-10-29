using KuaceMenu.Web.Filters;
using KuaceMenu.Web.Models;
using KuaceMenu.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace KuaceMenu.Web.Controllers;

[Authorize]
[ServiceFilter(typeof(TenantAuthorizationFilter))]
[Route("panel/qr")]
public class QrController : Controller
{
    private readonly ITenantContext _tenantContext;
    private readonly IQrCodeService _qrService;
    private readonly IOptions<DomainOptions> _domainOptions;

    public QrController(ITenantContext tenantContext, IQrCodeService qrService, IOptions<DomainOptions> domainOptions)
    {
        _tenantContext = tenantContext;
        _qrService = qrService;
        _domainOptions = domainOptions;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        var tenant = _tenantContext.Tenant!;
        var url = BuildUrl(tenant.Slug);
        ViewData["TargetUrl"] = url;
        return View(tenant);
    }

    [HttpGet("png")]
    public IActionResult DownloadPng()
    {
        var tenant = _tenantContext.Tenant!;
        var url = BuildUrl(tenant.Slug);
        var bytes = _qrService.GeneratePng(url);
        return File(bytes, "image/png", $"{tenant.Slug}-menu.png");
    }

    [HttpGet("pdf")]
    public IActionResult DownloadPdf()
    {
        var tenant = _tenantContext.Tenant!;
        var url = BuildUrl(tenant.Slug);
        var bytes = _qrService.GeneratePdf(url, tenant.Name);
        return File(bytes, "application/pdf", $"{tenant.Slug}-menu.pdf");
    }

    private string BuildUrl(string slug)
    {
        var baseDomain = _domainOptions.Value.BaseDomain;
        return $"https://{slug}.{baseDomain}/";
    }
}
