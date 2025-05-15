using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace GHLearning.EasyHangfire.AppOpenTelemetry.AuthorizationFilters;

public class DashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
	public bool Authorize([NotNull] DashboardContext context) => true;
}
