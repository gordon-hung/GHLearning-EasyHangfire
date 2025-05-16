using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using GHLearning.EasyHangfire.AppAuthorization.Models;

namespace GHLearning.EasyHangfire.AppAuthorization.Controllers;

public class LoginController(
	IOptions<AuthorizationWhitelistOptions> options) : Controller
{
	private readonly AuthorizationWhitelistOptions _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

	public IActionResult Index(string? error = null)
	{
		if (User?.Identity?.IsAuthenticated == true)
		{
			HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).GetAwaiter().GetResult();
		}

		ViewBag.ErrorMessage = error;

		return View();
	}

	[HttpPost]
	public async Task<IActionResult> LoginAsync(LoginViewModel login)
	{
		if (!ModelState.IsValid)
		{
			ViewBag.ErrorMessage = "請輸入帳號與密碼";
			return View("Index", login);
		}

		var personnel = _options.Personnels.FirstOrDefault(x => x.UserName == login.Account && x.Password == login.Password);
		if (personnel is null)
		{
			ViewBag.ErrorMessage = "帳號或密碼錯誤";
			return View("Index", login);
		}

		var claims = new List<Claim>
		{
			new(ClaimTypes.Name, personnel.UserName),
			new(ClaimTypes.Role, personnel.Roles)
		};
		var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

		await HttpContext.SignInAsync(
			CookieAuthenticationDefaults.AuthenticationScheme,
			new ClaimsPrincipal(claimsIdentity),
			new AuthenticationProperties
			{
				IsPersistent = true,
				ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1)
			}).ConfigureAwait(false);

		return Redirect("/hangfire");
	}
}
