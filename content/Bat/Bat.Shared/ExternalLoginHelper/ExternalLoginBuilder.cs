using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Bat.Shared.ExternalLoginHelper;

/// <summary>
/// Builder class for creating an instance of <see cref="ExternalLoginManager"/>.
/// </summary>
public class ExternalLoginBuilder
{
	/// <summary>
	/// Creates a new instance of <see cref="ExternalLoginBuilder"/>.
	/// </summary>
	public static ExternalLoginBuilder New()
	{
		return new ExternalLoginBuilder();
	}

	protected IDictionary<string, IConfigurationSection> ProvidersConfig { get; private set; } = new Dictionary<string, IConfigurationSection>();
	protected IDictionary<string, ExternalLoginProviderConfig> Providers { get; private set; } = new Dictionary<string, ExternalLoginProviderConfig>();
	protected bool ThrownOnInvalidConfig { get; private set; } = false;
	protected HttpClient? HttpClient { get; private set; } = default;

	protected ExternalLoginBuilder() { }

	public ExternalLoginManager Build()
	{
		var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<ExternalLoginBuilder>();
		IDictionary<string, ExternalLoginProviderConfig> finalProviders = new Dictionary<string, ExternalLoginProviderConfig>();

		if (ProvidersConfig.Count == 0 && Providers.Count == 0)
		{
			logger.LogWarning("No external login providers configuration provided.");
			return new ExternalLoginManager(finalProviders);
		}

		foreach (var (providerName, providerConfig) in ProvidersConfig)
		{
			var pconfig = providerConfig?.Get<ExternalLoginProviderConfig>()
				?? (ThrownOnInvalidConfig
					? throw new InvalidOperationException($"External login provider '{providerName}' configuration is invalid.")
					: null);
			if (pconfig != null)
			{
				finalProviders[providerName] = pconfig;
			}
		}
		foreach (var (providerName, providerConfig) in Providers)
		{
			if (providerConfig != null)
			{
				finalProviders[providerName] = providerConfig;
			}
		}

		return new ExternalLoginManager(finalProviders, HttpClient);
	}

	/// <summary>
	/// Configures the <see cref="HttpClient"/> to use for making HTTP requests.
	/// </summary>
	/// <param name="httpClient">The <see cref="HttpClient"/> to use for making HTTP requests.</param>
	/// <returns>The current instance of <see cref="ExternalLoginBuilder"/>.</returns>
	public ExternalLoginBuilder WithHttpClient(HttpClient httpClient)
	{
		HttpClient = httpClient;
		return this;
	}

	/// <summary>
	/// Configures whether to throw an exception when an invalid provider configuration is encountered.
	/// </summary>
	/// <param name="thrownOnInvalidConfig">Whether to throw an exception when an invalid provider configuration is encountered.</param>
	/// <returns>The current instance of <see cref="ExternalLoginBuilder"/>.</returns>
	public ExternalLoginBuilder WithThrownOnInvalidConfig(bool thrownOnInvalidConfig)
	{
		ThrownOnInvalidConfig = thrownOnInvalidConfig;
		return this;
	}

	/// <summary>
	/// Configures an external login provider.
	/// </summary>
	/// <param name="providerName">The name of the provider.</param>
	/// <param name="providerConfig">The provider configuration.</param>
	/// <returns>The current instance of <see cref="ExternalLoginBuilder"/>.</returns>
	public ExternalLoginBuilder WithProviderConfig(string providerName, IConfigurationSection providerConfig)
	{
		ProvidersConfig[providerName] = providerConfig;
		return this;
	}

	/// <summary>
	/// Configures an external login provider.
	/// </summary>
	/// <param name="providerName">The name of the provider.</param>
	/// <param name="providerConfig"></param>
	/// <returns>The current instance of <see cref="ExternalLoginBuilder"/>.</returns>
	public ExternalLoginBuilder WithProvider(string providerName, ExternalLoginProviderConfig providerConfig)
	{
		Providers[providerName] = providerConfig;
		return this;
	}

	/// <summary>
	/// Configures the external login providers.
	/// </summary>
	/// <param name="appConfig">The application configuration.</param>
	/// <returns>The current instance of <see cref="ExternalLoginBuilder"/>.</returns>
	/// <remarks>
	/// The configuration should be in the form of:
	/// <code>
	/// {
	///   "Authentication": {
	///     "provider-name-eg.Google": {
	///       "ClientId": "google-client-id",
	///       "ClientSecret": "google-client-secret"
	///     },
	///     ...
	/// }
	/// </code>
	/// </remarks>
	public ExternalLoginBuilder WithProvidersConfig(IConfiguration appConfig)
	{
		return WithProvidersConfig(appConfig, "Authentication");
	}

	/// <summary>
	/// Configures the external login providers.
	/// </summary>
	/// <param name="appConfig">The application configuration.</param>
	/// <param name="sectionName">The name of the section containing the providers configuration.</param>
	/// <returns>The current instance of <see cref="ExternalLoginBuilder"/>.</returns>
	/// <remarks>
	/// The configuration should be in the form of:
	/// <code>
	/// {
	///   "section-name": {
	///     "provider-name-eg.Google": {
	///       "ClientId": "google-client-id",
	///       "ClientSecret": "google-client-secret"
	///     },
	///     ...
	/// }
	/// </code>
	/// </remarks>
	public ExternalLoginBuilder WithProvidersConfig(IConfiguration appConfig, string sectionName)
	{
		var providersConfigSection = appConfig.GetSection(sectionName);
		providersConfigSection.GetChildren().ToList().ForEach(c => ProvidersConfig[c.Key] = c);
		return this;
	}

	/// <summary>
	/// Configures the external login providers.
	/// </summary>
	/// <param name="providers"></param>
	/// <returns>The current instance of <see cref="ExternalLoginBuilder"/>.</returns>
	public ExternalLoginBuilder WithProviders(IDictionary<string, ExternalLoginProviderConfig> providers)
	{
		foreach (var (providerName, providerConfig) in providers)
		{
			if (providerConfig != null)
			{
				Providers[providerName] = providerConfig;
			}
		}
		return this;
	}
}
