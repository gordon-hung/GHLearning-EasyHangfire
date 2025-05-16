using System.Diagnostics.CodeAnalysis;
using System.Net;
using Hangfire.Dashboard;

namespace GHLearning.EasyHangfire.AppAuthorization.AuthorizationFilters;

public class DashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
	public bool Authorize([NotNull] DashboardContext context)
	{
		var httpContext = context.GetHttpContext();

		return httpContext.User?.Identity?.IsAuthenticated == true;
	}
}
