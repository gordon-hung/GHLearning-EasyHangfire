namespace GHLearning.EasyHangfire.AppAuthorization.Models;

public class ErrorViewModel
{
	public string? RequestId { get; set; }

	public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
