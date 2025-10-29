using System.Threading.Tasks;
using KuaceMenu.Web.Filters;
using KuaceMenu.Web.Models;
using KuaceMenu.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace KuaceMenu.Web.Controllers;

[Authorize]
[ServiceFilter(typeof(TenantAuthorizationFilter))]
[Route("payment")]
public class PaymentsController : Controller
{
    private readonly ITenantContext _tenantContext;
    private readonly IPaymentService _paymentService;
    private readonly UserManager<AppUser> _userManager;

    public PaymentsController(ITenantContext tenantContext, IPaymentService paymentService, UserManager<AppUser> userManager)
    {
        _tenantContext = tenantContext;
        _paymentService = paymentService;
        _userManager = userManager;
    }

    [HttpGet("checkout")]
    public async Task<IActionResult> Checkout()
    {
        var tenant = _tenantContext.Tenant!;
        var user = await _userManager.GetUserAsync(User);
        var session = await _paymentService.CreateCheckoutSessionAsync(tenant, user!, Request);
        ViewData["CheckoutHtml"] = session.HtmlContent;
        ViewData["Token"] = session.Token;
        return View(tenant);
    }

    [HttpPost("simulate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SimulateSuccess(string token)
    {
        var tenant = _tenantContext.Tenant!;
        await _paymentService.CompletePaymentAsync(tenant.Id, token, success: true);
        TempData["Success"] = "Ödeme başarıyla tamamlandı";
        return RedirectToAction("Index", "Panel");
    }

    [AllowAnonymous]
    [HttpPost("callback")]
    public async Task<IActionResult> Callback([FromForm] PaymentCallbackModel model)
    {
        var payment = await _paymentService.CompletePaymentAsync(model.TenantId, model.Token, model.Status == "success", model.RawJson);
        if (payment is null)
        {
            return NotFound();
        }

        return Ok();
    }

    [HttpGet("history")]
    public async Task<IActionResult> History()
    {
        var tenant = _tenantContext.Tenant!;
        var payments = await _paymentService.GetPaymentsAsync(tenant.Id);
        return View(payments);
    }
}

public class PaymentCallbackModel
{
    public int TenantId { get; set; }
    public string Token { get; set; } = string.Empty;
    public string Status { get; set; } = "failed";
    public string? RawJson { get; set; }
}
