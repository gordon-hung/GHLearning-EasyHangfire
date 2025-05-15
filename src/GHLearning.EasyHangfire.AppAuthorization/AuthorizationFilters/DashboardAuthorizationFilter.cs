using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using Hangfire.Dashboard;
using Microsoft.Extensions.Options;

namespace GHLearning.EasyHangfire.AppAuthorization.AuthorizationFilters;

public class DashboardAuthorizationFilter(IOptions<AuthorizationWhitelistOptions> options) : IDashboardAuthorizationFilter
{
	private readonly AuthorizationWhitelistOptions _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

	public bool Authorize([NotNull] DashboardContext context)
	{
		var httpContext = context.GetHttpContext();
		var header = httpContext.Request.Headers.Authorization;

		// 如果沒有授權標頭，直接返回 401
		if (string.IsNullOrWhiteSpace(header))
		{
			SetChallengeResponse(httpContext);
			return false;
		}

		var authValues = TryParseAuthorizationHeader(header!);
		if (authValues == null || !ValidateBasicAuthScheme(authValues))
		{
			SetChallengeResponse(httpContext);
			return false;
		}

		var credentials = DecodeCredentials(authValues.Parameter!);
		if (credentials == null || !ValidateCredentials(credentials.Value))
		{
			SetChallengeResponse(httpContext);
			return false;
		}

		return true;
	}

	/// <summary>
	/// Tries the parse authorization header.
	/// </summary>
	/// <param name="header">The header.</param>
	/// <returns></returns>
	private static AuthenticationHeaderValue? TryParseAuthorizationHeader(string header)
	{
		try
		{
			return AuthenticationHeaderValue.Parse(header);
		}
		catch
		{
			return null;  // 如果解析失敗，返回 null
		}
	}

	/// <summary>
	/// Validates the basic authentication scheme.
	/// </summary>
	/// <param name="authValues">The authentication values.</param>
	/// <returns></returns>
	private static bool ValidateBasicAuthScheme(AuthenticationHeaderValue authValues)
	{
		return "Basic".Equals(authValues.Scheme, StringComparison.InvariantCultureIgnoreCase);
	}

	/// <summary>
	/// Decodes the credentials.
	/// </summary>
	/// <param name="base64Credentials">The base64 credentials.</param>
	/// <returns></returns>
	private static (string Username, string Password)? DecodeCredentials(string base64Credentials)
	{
		try
		{
			var parameter = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64Credentials));
			var parts = parameter.Split(':');
			return parts.Length < 2 ? null : (parts[0], parts[1]);
		}
		catch
		{
			return null;  // 如果解碼失敗，返回 null
		}
	}

	/// <summary>
	/// Sets the challenge response.
	/// </summary>
	/// <param name="httpContext">The HTTP context.</param>
	private static void SetChallengeResponse(HttpContext httpContext)
	{
		httpContext.Response.StatusCode = 401;
		httpContext.Response.Headers.Append("WWW-Authenticate", "Basic realm=\"Hangfire Dashboard\"");
		httpContext.Response.WriteAsync("Authentication is required.");
	}

	/// <summary>
	/// Validates the credentials.
	/// </summary>
	/// <param name="credentials">The credentials.</param>
	/// <returns></returns>
	private bool ValidateCredentials((string Username, string Password) credentials)
	{
		return _options.Personnels
			.FirstOrDefault(x => x.UserName == credentials.Username && x.Password == credentials.Password) != null;
	}
}
