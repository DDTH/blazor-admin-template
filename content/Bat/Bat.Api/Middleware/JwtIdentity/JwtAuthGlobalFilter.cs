using Microsoft.AspNetCore.Mvc.Filters;

namespace Bat.Api.Middleware.JwtIdentity;

/// <summary>
/// Action filter that checks if the executing action is marked with JwtAuthorizeAttribute.
/// If yes, it will authorize the request based on the rules from the attached JwtAuthorizeAttribute.
/// </summary>
public class JwtAuthGlobalFilter : IAsyncActionFilter
{
	public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next) => throw new NotImplementedException();
}
