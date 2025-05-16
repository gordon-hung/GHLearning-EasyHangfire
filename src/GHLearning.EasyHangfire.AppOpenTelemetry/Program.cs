using Hangfire;
using GHLearning.EasyHangfire.AppOpenTelemetry.AuthorizationFilters;
using GHLearning.EasyHangfire.AppOpenTelemetry.JobHandlers;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Learn more about configuring Hangfire at https://docs.hangfire.io/en/latest/
builder.Services.AddHangfire(configuration => configuration
	.UseInMemoryStorage())
	.AddHangfireServer(options => options.SchedulePollingInterval = TimeSpan.FromSeconds(1));

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<PerSecondJobHandler>();
builder.Services.AddSingleton<PerMinuteJobHandler>();
builder.Services.AddSingleton<PerHourJobHandler>();

//Learn more about configuring OpenTelemetry at https://learn.microsoft.com/zh-tw/dotnet/core/diagnostics/observability-with-otel
builder.Services.AddOpenTelemetry()
	.ConfigureResource(resource => resource
	.AddService("EasyHangfire-AppOpenTelemetry(local)"))
	.UseOtlpExporter(OtlpExportProtocol.Grpc, new Uri("http://127.0.0.1:4317"))
	.WithMetrics(metrics => metrics
		.AddMeter("GHLearning.")
		.AddAspNetCoreInstrumentation()
		.AddRuntimeInstrumentation()
		.AddProcessInstrumentation()
		.AddPrometheusExporter())
	.WithTracing(tracing => tracing
		.AddHangfireInstrumentation()
		.AddEntityFrameworkCoreInstrumentation()
		.AddHttpClientInstrumentation()
		.AddAspNetCoreInstrumentation(options => options.Filter = (httpContext) => !httpContext.Request.Path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase) &&
				!httpContext.Request.Path.StartsWithSegments("/live", StringComparison.OrdinalIgnoreCase) &&
				!httpContext.Request.Path.StartsWithSegments("/healthz", StringComparison.OrdinalIgnoreCase) &&
				!httpContext.Request.Path.StartsWithSegments("/metrics", StringComparison.OrdinalIgnoreCase) &&
				!httpContext.Request.Path.StartsWithSegments("/favicon.ico", StringComparison.OrdinalIgnoreCase) &&
				!httpContext.Request.Path.Value!.Equals("/api/events/raw", StringComparison.OrdinalIgnoreCase) &&
				!httpContext.Request.Path.Value!.EndsWith(".js", StringComparison.OrdinalIgnoreCase) &&
				!httpContext.Request.Path.StartsWithSegments("/_vs", StringComparison.OrdinalIgnoreCase)&&
				!httpContext.Request.Path.StartsWithSegments("/hangfire", StringComparison.OrdinalIgnoreCase)));

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
