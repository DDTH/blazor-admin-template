using Bat.Shared.Bootstrap;
using System.Text.Json;

namespace Bat.Blazor.Bootstrap;

/// <summary>
/// Bootstrapper that handle errors that have not been caught by other routes.
/// </summary>
[Bootstrapper]
public class Fallback404Bootstrapper
{

	public static void DecorateApp(WebApplication app)
	{
		app.UseStatusCodePages(new StatusCodePagesOptions()
		{
			HandleAsync = async context =>
			{
				context.HttpContext.Response.ContentType = "application/json";
				var response = new
				{
					status = context.HttpContext.Response.StatusCode,
				};
				await context.HttpContext.Response.BodyWriter.WriteAsync(JsonSerializer.SerializeToUtf8Bytes(response));

			}
		});
	}
}
