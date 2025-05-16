using GHLearning.EasyHangfire.AppDelayedJobs.AuthorizationFilters;
using GHLearning.EasyHangfire.AppDelayedJobs.JobHandlers;
using Hangfire;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Learn more about configuring Hangfire at https://docs.hangfire.io/en/latest/
builder.Services.AddHangfire(configuration => configuration
	.UseInMemoryStorage())
	.AddHangfireServer(options => options.SchedulePollingInterval = TimeSpan.FromSeconds(15));

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<ScheduleHandler>();

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

BackgroundJob.Schedule(
	(ScheduleHandler scheduleHandler) => scheduleHandler.ExecuteAsync(),
	TimeSpan.FromSeconds(3));

app.Run();
