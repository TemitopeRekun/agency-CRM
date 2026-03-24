using Hangfire.Dashboard;
using System.Diagnostics.CodeAnalysis;

namespace Crm.Infrastructure.Security;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize([NotNull] DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // For development, allow local requests
        if (httpContext.Request.Host.Host == "localhost" || httpContext.Request.Host.Host == "127.0.0.1")
        {
            return true;
        }

        // In production, enforce authentication and Admin role
        return httpContext.User.Identity?.IsAuthenticated == true && 
               httpContext.User.IsInRole("Admin");
    }
}
