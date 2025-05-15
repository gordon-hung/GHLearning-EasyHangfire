using GHLearning.EasyHangfire.AppRecurringJobs.AuthorizationFilters;
using GHLearning.EasyHangfire.AppRecurringJobs.JobHandlers;
using Hangfire;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Learn more about configuring  Sentry at https://docs.hangfire.io/en/latest/
builder.Services.AddHangfire(configuration => configuration
	.UseInMemoryStorage())
	.AddHangfireServer(options => options.SchedulePollingInterval = TimeSpan.FromSeconds(1));

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<PerSecondJobHandler>();
builder.Services.AddSingleton<PerMinuteJobHandler>();
builder.Services.AddSingleton<PerHourJobHandler>();

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
		new DashboardAuthorizationFilter()
	],
	IsReadOnlyFunc = (context) => false
});

app.MapControllers();

RecurringJob.AddOrUpdate(
	nameof(PerSecondJobHandler),
	(PerSecondJobHandler handler) => handler.ExecuteAsync(),
	"*/1 * * * * *",
	new RecurringJobOptions
	{
		TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Shanghai")
	}
);

RecurringJob.AddOrUpdate(
	nameof(PerMinuteJobHandler),
	(PerMinuteJobHandler handler) => handler.ExecuteAsync(),
	"0 */1 * * * *",
	new RecurringJobOptions
	{
		TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Shanghai")
	}
);

RecurringJob.AddOrUpdate(
	nameof(PerHourJobHandler),
	(PerHourJobHandler handler) => handler.ExecuteAsync(),
	"0 0 */1 * * *",
	new RecurringJobOptions
	{
		TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Shanghai")
	}
);

app.Run();
