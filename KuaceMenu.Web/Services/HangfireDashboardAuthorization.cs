using System;
using System.Linq;
using System.Text;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace KuaceMenu.Web.Services;

public class HangfireDashboardAuthorization : IDashboardAuthorizationFilter
{
    private readonly string _username;
    private readonly string _password;

    public HangfireDashboardAuthorization(IConfiguration configuration)
    {
        _username = configuration["Hangfire:DashboardUser"] ?? "admin";
        _password = configuration["Hangfire:DashboardPassword"] ?? "admin";
    }

    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        var header = httpContext.Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrEmpty(header) || !header.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            Challenge(httpContext);
            return false;
        }

        var encoded = header.Substring("Basic ".Length).Trim();
        var credentialBytes = Convert.FromBase64String(encoded);
        var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);
        if (credentials.Length != 2)
        {
            Challenge(httpContext);
            return false;
        }

        if (credentials[0] == _username && credentials[1] == _password)
        {
            return true;
        }

        Challenge(httpContext);
        return false;
    }

    private static void Challenge(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Hangfire Dashboard\"";
    }
}
