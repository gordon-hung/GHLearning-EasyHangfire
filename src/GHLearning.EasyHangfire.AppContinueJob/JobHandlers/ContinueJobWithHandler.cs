namespace GHLearning.EasyHangfire.AppContinueJob.JobHandlers;

public class ContinueJobWithHandler(
	ILogger<ContinueJobWithHandler> logger,
	TimeProvider timeProvider)
{
	private int _count;

	public Task ExecuteAsync()
	{
		_count++;
		logger.LogInformation("由HangFire排程發送，時間:{dateAt} Count:{count}", timeProvider.GetUtcNow(), _count);
		return Task.Delay(500);
	}
}
