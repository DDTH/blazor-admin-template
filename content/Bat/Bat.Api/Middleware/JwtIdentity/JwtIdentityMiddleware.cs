namespace Bat.Api.Middleware.JwtIdentity;

/// <summary>
/// Middleware that decodes the JWT token in the request Authorization header (if any) and attaches
/// the user-id/user to the HttpContext.Items collection to make it accessible within the scope of the current request.
/// </summary>
public class JwtIdentityMiddleware
{
}
