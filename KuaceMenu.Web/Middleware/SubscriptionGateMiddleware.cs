using KuaceMenu.Web.Models;

public class SubscriptionGateMiddleware : IMiddleware
{
	private static readonly PathString[] AllowPrefixes = new[]
	{
		new PathString("/identity"),   // Identity UI
        new PathString("/account"),    // varsa custom hesap yolları
        new PathString("/hangfire"),
		new PathString("/css"),
		new PathString("/js"),
		new PathString("/images"),
		new PathString("/lib")
	};

	public async Task InvokeAsync(HttpContext context, RequestDelegate next)
	{
		var path = context.Request.Path;

		// Serbest bırakılan yollar
		if (AllowPrefixes.Any(p => path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase)))
		{
			await next(context);
			return;
		}

		// Panel zaten Auth istiyor, gate paneli bloklamasın
		if (path.StartsWithSegments("/panel", StringComparison.OrdinalIgnoreCase))
		{
			await next(context);
			return;
		}

		// Tenant yoksa (localhost’ta ?tenant=slug verilmemişse) gate devreye girmesin
		if (!context.Items.TryGetValue("Tenant", out var tenantObj) || tenantObj is not Tenant tenant)
		{
			await next(context);
			return;
		}

		if (!tenant.IsPublicMenuEnabled || tenant.SubscriptionStatus != SubscriptionStatus.Active)
		{
			context.Response.ContentType = "text/html; charset=utf-8";
			await context.Response.WriteAsync("<h1>Bu işletmenin menüsü şu an kullanım dışı.</h1>");
			return;
		}

		await next(context);
	}
}
