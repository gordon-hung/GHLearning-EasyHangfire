using Hangfire;
using GHLearning.EasyHangfire.AppSentry.AuthorizationFilters;
using GHLearning.EasyHangfire.AppSentry.JobHandlers;
using Serilog;
using Serilog.Exceptions;
using Sentry.Hangfire;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Learn more about configuring Hangfire at https://docs.hangfire.io/en/latest/
builder.Services.AddHangfire(configuration =>
{
	configuration.UseInMemoryStorage();
	// Learn more about configuring  Sentry at https://docs.sentry.io/platforms/dotnet/guides/aspnet/crons/hangfire/
	configuration.UseSentry();
})
	.AddHangfireServer(options => options.SchedulePollingInterval = TimeSpan.FromSeconds(15));

// Learn more about configuring  Serilog at https://github.com/serilog/serilog/wiki/Configuration-Basics
Log.Logger = new LoggerConfiguration()
	.ReadFrom.Configuration(builder.Configuration)
	.Enrich.WithExceptionDetails()
	.Enrich.WithProperty("ApplicationName", builder.Environment.ApplicationName)
	.Enrich.WithProperty("EnvironmentName", builder.Environment.EnvironmentName)
	.Enrich.WithProperty("RuntimeId", SequentialGuid.SequentialGuidGenerator.Instance.NewGuid())
	.Enrich.WithProperty("ApplicationStartAt", DateTimeOffset.UtcNow.ToString("u"))
	.Enrich.WithTraceIdentifier()
	.MinimumLevel.Debug()
	.CreateLogger();

builder.Logging.ClearProviders().AddSerilog();

builder.Host.UseSerilog();

// Learn more about configuring  Sentry at https://docs.sentry.io/platforms/dotnet/guides/aspnetcore/
builder.WebHost.UseSentry();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<PerMinuteJobHandler>();

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
	nameof(PerMinuteJobHandler),
	(PerMinuteJobHandler handler) => handler.ExecuteAsync(),
	"0 */1 * * * *",
	new RecurringJobOptions
	{
		TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Shanghai")
	}
);

app.Run();
