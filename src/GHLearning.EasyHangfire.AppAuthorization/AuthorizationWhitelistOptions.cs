namespace GHLearning.EasyHangfire.AppAuthorization;

public record AuthorizationWhitelistOptions
{
	public IReadOnlyCollection<Personnel> Personnels { get; set; } = default!;
}

public record Personnel(
	string UserName,
	string Password,
	string Roles);
