using System;
using System.Security.Claims;
using System.Threading.Tasks;
using KuaceMenu.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace KuaceMenu.Web.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class TenantAuthorizationFilter : Attribute, IAsyncActionFilter
{
    private readonly bool _allowAdmin;

    public TenantAuthorizationFilter(bool allowAdmin = true)
    {
        _allowAdmin = allowAdmin;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var tenantContext = context.HttpContext.RequestServices.GetRequiredService<ITenantContext>();
        var tenant = tenantContext.Tenant;
        if (tenant is null)
        {
            context.Result = new NotFoundResult();
            return;
        }

        var user = context.HttpContext.User;
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            context.Result = new ForbidResult();
            return;
        }

        if (tenant.OwnerUserId != userId && !(_allowAdmin && user.IsInRole("Admin")))
        {
            context.Result = new ForbidResult();
            return;
        }

        await next();
    }
}
