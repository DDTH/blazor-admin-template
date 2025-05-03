using Bat.Shared.Bootstrap;
using System.Text.Json;

namespace Bat.Api.Bootstrap;

/// <summary>
/// Bootstrapper that configures handler to catch errors not catching by other routes.
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
