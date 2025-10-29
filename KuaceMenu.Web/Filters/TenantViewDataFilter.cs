using System.Threading.Tasks;
using KuaceMenu.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace KuaceMenu.Web.Filters;

public class TenantViewDataFilter : IAsyncActionFilter
{
    private readonly ITenantContext _tenantContext;

    public TenantViewDataFilter(ITenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var resultContext = await next();
        if (resultContext.Controller is Controller controller && _tenantContext.Tenant is not null)
        {
            controller.ViewData["Tenant"] = _tenantContext.Tenant;
        }
    }
}
