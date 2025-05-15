using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace GHLearning.EasyHangfire.AppDelayedJobs.AuthorizationFilters;

public class DashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
	public bool Authorize([NotNull] DashboardContext context) => true;
}
