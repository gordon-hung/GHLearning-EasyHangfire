using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using GHLearning.EasyHangfire.AppAuthorization;
using GHLearning.EasyHangfire.AppAuthorization.AuthorizationFilters;
using GHLearning.EasyHangfire.AppAuthorization.JobHandlers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Learn more about configuring Authentication at https://learn.microsoft.com/zh-tw/aspnet/core/security/authentication/cookie?view=aspnetcore-9.0
builder.Services
	.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
	.AddCookie(options =>
	{
		options.LoginPath = "/Login/Index";
		options.ExpireTimeSpan = TimeSpan.FromHours(1);
	});

// Learn more about configuring Hangfire at https://docs.hangfire.io/en/latest/
builder.Services.AddHangfire(configuration => configuration
	.UseInMemoryStorage())
	.AddHangfireServer(options => options.SchedulePollingInterval = TimeSpan.FromSeconds(15));

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<PerMinuteJobHandler>();
builder.Services
	.AddOptions<AuthorizationWhitelistOptions>()
	.Configure((options) => builder.Configuration.GetSection("AuthorizationWhitelist").Bind(options));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
	Authorization =
	[
		new DashboardAuthorizationFilter()
	],
	IsReadOnlyFunc = (context) =>
	{
		var httpContext = context.GetHttpContext();

		var role = httpContext.User.Claims
			.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;

		return !string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase);
	}
});

app.MapStaticAssets();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Login}/{action=Index}/{id?}")
	.WithStaticAssets();

RecurringJob.AddOrUpdate(
	nameof(PerMinuteJobHandler),
	(PerMinuteJobHandler handler) => handler.ExecuteAsync(),
	"0 */1 * * * *",
	new RecurringJobOptions
	{
		TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Shanghai")
	}
);

app.Run();
