using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using KuaceMenu.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KuaceMenu.Web.Middleware;

public class SubdomainTenantResolverMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ITenantService _tenantService;
    private readonly ITenantContext _tenantContext;
    private readonly DomainOptions _options;
    private readonly ILogger<SubdomainTenantResolverMiddleware> _logger;

    public SubdomainTenantResolverMiddleware(
        RequestDelegate next,
        ITenantService tenantService,
        ITenantContext tenantContext,
        IOptions<DomainOptions> options,
        ILogger<SubdomainTenantResolverMiddleware> logger)
    {
        _next = next;
        _tenantService = tenantService;
        _tenantContext = tenantContext;
        _options = options.Value;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var host = context.Request.Host.Host;
        string? slug = null;

        if (IsLocalhost(host))
        {
            slug = context.Request.Query["tenant"].FirstOrDefault();
        }
        else
        {
            var segments = host.Split('.');
            var baseSegments = _options.BaseDomain.Split('.');
            if (segments.Length > baseSegments.Length)
            {
                slug = segments[0];
            }
        }

        if (!string.IsNullOrWhiteSpace(slug))
        {
            var tenant = await _tenantService.GetTenantBySlugAsync(slug);
            if (tenant is not null)
            {
                context.Items["Tenant"] = tenant;
                _tenantContext.Tenant = tenant;
                _tenantContext.Slug = slug;
            }
            else
            {
                _logger.LogWarning("Tenant bulunamadı: {Slug}", slug);
            }
        }

        await _next(context);
    }

    private static bool IsLocalhost(string host)
    {
        return string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase)
            || host.StartsWith("127.")
            || host.Equals(IPAddress.Loopback.ToString(), StringComparison.OrdinalIgnoreCase);
    }
}
