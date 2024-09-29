using Bat.Shared.Bootstrap;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace Bat.Api.Bootstrap;

/// <summary>
/// Built-in bootstrapper that initializes Swagger/OpenAPI services and UI.
/// </summary>
/// <remarks>
///		This bootstrapper must be invoked after the ControllersBootstrapper to ensure that all controllers are registered before Swagger scans them.
/// </remarks>
[Bootstrapper(Priority = 2000)]
public class SwaggerBootstrapper
{
	public static void ConfigureBuilder(WebApplicationBuilder appBuilder)
	{
		// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
		appBuilder.Services.AddSwaggerGen(options =>
		{
			options.SwaggerDoc("v1", new OpenApiInfo
			{
				Version = "v1",
				Title = "BAT Backend API",
				Description = "Blazor Admin Template - API Backend.",
				// TermsOfService = new Uri("https://example.com/terms"),
				Contact = new OpenApiContact
				{
					Name = "GitHub Repo",
					Url = new Uri("https://github.com/DDTH/blazor-admin-template")
				},
				License = new OpenApiLicense
				{
					Name = "MIT - License",
					Url = new Uri("https://github.com/DDTH/blazor-admin-template/blob/main/LICENSE.md")
				}
			});

			// Define the OAuth2.0 scheme that's in use (i.e., Implicit Flow)
			options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
			{
				Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
				Name = "Authorization",
				In = ParameterLocation.Header,
				Type = SecuritySchemeType.ApiKey,
				Scheme = "Bearer"
			});

			options.AddSecurityRequirement(new OpenApiSecurityRequirement()
			{
				{
				   new OpenApiSecurityScheme
				   {
						Reference = new OpenApiReference
						{
							Type = ReferenceType.SecurityScheme,
							Id = "Bearer"
						},
						Scheme = "oauth2",
						Name = "Bearer",
						In = ParameterLocation.Header,
				   },
				   new List<string>()
				}
			});

			// https://learn.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-8.0&tabs=visual-studio-code#xml-comments
			var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
			options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
		});
	}

	public static void DecorateApp(WebApplication app, ILogger<SwaggerBootstrapper> logger)
	{
		var tryParse = bool.TryParse(Environment.GetEnvironmentVariable(Globals.ENV_ENABLE_SWAGGER_UI), out var enableSwaggerUi);
		if (!app.Environment.IsDevelopment() && (!tryParse || !enableSwaggerUi)) return;

		app.UseSwagger();
		app.UseSwaggerUI();
		logger.LogInformation("Swagger UI enabled at /swagger");
	}
}
