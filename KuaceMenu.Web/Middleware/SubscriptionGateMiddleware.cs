using System;
using System.Threading.Tasks;
using KuaceMenu.Web.Models;
using Microsoft.AspNetCore.Http;

namespace KuaceMenu.Web.Middleware;

public class SubscriptionGateMiddleware
{
    private readonly RequestDelegate _next;

    public SubscriptionGateMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/panel", StringComparison.OrdinalIgnoreCase))
        {
            if (context.Items.TryGetValue("Tenant", out var tenantObj) && tenantObj is Tenant tenant)
            {
                if (!tenant.IsPublicMenuEnabled || tenant.SubscriptionStatus != SubscriptionStatus.Active)
                {
                    context.Response.ContentType = "text/html; charset=utf-8";
                    await context.Response.WriteAsync("<h1>Bu işletmenin menüsü şu an kullanım dışı.</h1>");
                    return;
                }
            }
        }

        await _next(context);
    }
}
