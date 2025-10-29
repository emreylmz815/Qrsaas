using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using KuaceMenu.Web.Models;
using KuaceMenu.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KuaceMenu.Web.Middleware;

public class SubdomainTenantResolverMiddleware : IMiddleware
{
	private readonly ITenantService _tenantService;
	private readonly ITenantContext _tenantContext;
	private readonly DomainOptions _options;
	private readonly ILogger<SubdomainTenantResolverMiddleware> _logger;

	public SubdomainTenantResolverMiddleware(
		ITenantService tenantService,
		ITenantContext tenantContext,
		IOptions<DomainOptions> options,
		ILogger<SubdomainTenantResolverMiddleware> logger)
	{
		_tenantService = tenantService;
		_tenantContext = tenantContext;
		_options = options.Value;
		_logger = logger;
	}

	public async Task InvokeAsync(HttpContext context, RequestDelegate next)
	{
		var host = context.Request.Host.Host;
		string? slug = null;

		if (IsLocalhost(host))
			slug = context.Request.Query["tenant"].FirstOrDefault();

#if DEBUG
		if (string.IsNullOrWhiteSpace(slug))
			slug = "demo"; // dev kolaylığı
#endif

		if (!string.IsNullOrWhiteSpace(slug))
		{
			var tenant = await _tenantService.GetTenantBySlugAsync(slug);
			if (tenant is not null)
			{
				context.Items["Tenant"] = tenant;
				_tenantContext.Tenant = tenant;
				_tenantContext.Slug = slug;
			}
		}

		await next(context);
	}

	private static bool IsLocalhost(string host)
	{
		return string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase)
			|| host.StartsWith("127.")
			|| host.Equals(IPAddress.Loopback.ToString(), StringComparison.OrdinalIgnoreCase);
	}
}
