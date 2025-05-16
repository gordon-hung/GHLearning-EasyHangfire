namespace GHLearning.EasyHangfire.AppAuthorization.Models;

public record LoginViewModel
{
	public required string Account { get; init; }
	public required string Password { get; init; }
}
