using Sentry.Hangfire;

namespace GHLearning.EasyHangfire.AppSentry.JobHandlers;

public class PerMinuteJobHandler(
	ILogger<PerMinuteJobHandler> logger,
	TimeProvider timeProvider)
{
	private int _count = 0;

	[SentryMonitorSlug("per-minute-job-handler")]
	public Task ExecuteAsync()
	{
		_count++;
		logger.LogInformation("由HangFire排程發送，時間:{dateAt} Count:{count}", timeProvider.GetUtcNow(), _count);
		return Task.Delay(500);
	}
}
