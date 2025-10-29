using System.IO;
using Hangfire;
using Hangfire.SqlServer;
using KuaceMenu.Web.Data;
using KuaceMenu.Web.Filters;
using KuaceMenu.Web.Logging;
using KuaceMenu.Web.Middleware;
using KuaceMenu.Web.Models;
using KuaceMenu.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
var services = builder.Services;

// ensure logs directory
var logPath = Path.Combine(AppContext.BaseDirectory, "logs", "kuacemenu.log");
Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddProvider(new SimpleFileLoggerProvider(logPath));

services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

services.AddHangfire(config =>
{
	config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
		  .UseSimpleAssemblyNameTypeSerializer()
		  .UseRecommendedSerializerSettings()
		  .UseSqlServerStorage(configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
		  {
			  PrepareSchemaIfNecessary = true
		  });
});
services.AddHangfireServer();

services.AddIdentity<AppUser, IdentityRole>(options =>
{
	options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();
// .AddDefaultUI(); // hazýr Identity UI kullanacaksan aç

services.AddControllersWithViews(options =>
{
	options.Filters.Add<TenantViewDataFilter>();
});
services.AddRazorPages();

services.AddHttpContextAccessor();
services.AddScoped<ITenantContext, TenantContext>();
services.AddScoped<ITenantService, TenantService>();
services.AddScoped<IMenuService, MenuService>();
services.AddScoped<IPaymentService, PaymentService>();
services.AddScoped<IEmailSender, SmtpEmailSender>();
services.AddSingleton<IQrCodeService, QrCodeService>();
services.AddSingleton<IClock, SystemClock>();
services.AddScoped<TenantAuthorizationFilter>();
services.AddScoped<SubdomainTenantResolverMiddleware>();
services.AddScoped<SubscriptionGateMiddleware>();

services.Configure<IyzicoSettings>(configuration.GetSection("Iyzico"));
services.Configure<EmailSettings>(configuration.GetSection("Email"));
services.Configure<DomainOptions>(configuration.GetSection("Domain"));

// services.AddHostedService<HangfireBootstrapService>(); // gereksiz ise kaldýr

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// tenant -> subscription -> auth/authorize sýrasý da tercih edilebilir;
// ama pratikte þu sýra net çalýþýr:
app.UseMiddleware<SubdomainTenantResolverMiddleware>();
app.UseMiddleware<SubscriptionGateMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

var dashboardOptions = new DashboardOptions
{
	Authorization = new[] { new HangfireDashboardAuthorization(configuration) }
};
app.MapHangfireDashboard("/hangfire", dashboardOptions);

app.MapControllerRoute(
	name: "panel",
	pattern: "panel/{action=Index}/{id?}",
	defaults: new { controller = "Panel" });

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
	// migrate + seed
	await db.Database.MigrateAsync();
	await SeedData.EnsureSeedAsync(scope.ServiceProvider);

	var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
	recurringJobManager.AddOrUpdate<SubscriptionCheckJob>(
		"subscription-check",
		job => job.RunAsync(),
		Cron.Daily);
}

app.Run();
