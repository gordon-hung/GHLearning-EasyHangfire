using GHLearning.EasyHangfire.AppContinueJob.AuthorizationFilters;
using GHLearning.EasyHangfire.AppContinueJob.JobHandlers;
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
builder.Services.AddSingleton<EnqueueHandler>();
builder.Services.AddSingleton<ContinueJobWithHandler>();

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

var jobId = BackgroundJob.Enqueue((EnqueueHandler handler) => handler.ExecuteAsync());
BackgroundJob.ContinueJobWith(jobId, (ContinueJobWithHandler handler) => handler.ExecuteAsync());

app.Run();
