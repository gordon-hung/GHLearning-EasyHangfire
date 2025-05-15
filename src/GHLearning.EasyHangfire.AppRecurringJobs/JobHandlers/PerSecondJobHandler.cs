namespace GHLearning.EasyHangfire.AppRecurringJobs.JobHandlers;

public class PerSecondJobHandler(
	ILogger<PerSecondJobHandler> logger,
	TimeProvider timeProvider)
{
	private int _count = 0;

	public Task ExecuteAsync()
	{
		_count++;
		logger.LogInformation("由HangFire排程發送，時間:{dateAt} Count:{count}", timeProvider.GetUtcNow(), _count);
		return Task.Delay(500);
	}
}
