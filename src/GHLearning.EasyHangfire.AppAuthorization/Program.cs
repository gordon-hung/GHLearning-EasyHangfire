using GHLearning.EasyHangfire.AppAuthorization;
using GHLearning.EasyHangfire.AppAuthorization.AuthorizationFilters;
using GHLearning.EasyHangfire.AppAuthorization.JobHandlers;

using Hangfire;
using Hangfire.Dashboard;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Learn more about configuring  Sentry at https://docs.hangfire.io/en/latest/
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
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
	Authorization =
	[
		new DashboardAuthorizationFilter(app.Services.GetRequiredService<IOptions<AuthorizationWhitelistOptions>>())
	],
	IsReadOnlyFunc = (context) =>
	{
		var options = app.Services.GetRequiredService<IOptions<AuthorizationWhitelistOptions>>().Value;
		var httpContext = context.GetHttpContext();

		var header = httpContext.Request.Headers.Authorization;

		if (string.IsNullOrWhiteSpace(header))
		{
			return true;
		}

		var authValues = System.Net.Http.Headers.AuthenticationHeaderValue.Parse(header!);

		if (!"Basic".Equals(authValues.Scheme, StringComparison.InvariantCultureIgnoreCase))
		{
			return true;
		}

		if (string.IsNullOrWhiteSpace(authValues.Parameter))
		{
			return true;
		}

		var parameter = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(authValues.Parameter));
		var parts = parameter.Split(':');

		var username = parts[0];

		var personnel = options.Personnels.FirstOrDefault(x => x.UserName == username);

		return !personnel?.Roles.Equals("admin", StringComparison.OrdinalIgnoreCase) ?? false;
	}
});

app.MapControllers();

app.MapGet("/", (HttpContext httpContext) =>
{
	// 清除 Authorization 標頭
	httpContext.Request.Headers.Clear();

	httpContext.Response.StatusCode = 401;
	httpContext.Response.Headers.Append("WWW-Authenticate", "Basic realm=\"Hangfire Dashboard\"");
	httpContext.Response.WriteAsync("Authentication is required.");
});

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
