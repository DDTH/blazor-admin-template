namespace Bat.Api.Helpers;

/// <summary>
/// An application-wide global variables and repository.
/// </summary>
/// <remarks>
///		Use DI whenever possible. This global repository is meant for items that are needed in context where DI is not optimal.
/// </remarks>
public class Global
{
	/// <summary>
	/// Reference to the WebApplication instance.
	/// </summary>
	public static WebApplication? App { get; set; }

	/// <summary>
	/// Set to true when the server is ready to handle requests.
	/// </summary>
	public static bool Ready { get; set; } = false;

	/// <summary>
	/// Default name of the HttpContext item for storing the user ID.
	/// </summary>
	public const string HTTP_CTX_ITEM_USERID_DEFAULT = "UserId";

	/// <summary>
	/// Name of the Environment variable for enabling Swagger UI.
	/// </summary>
	public const string ENV_ENABLE_SWAGGER_UI = "ENABLE_SWAGGER_UI";

	/// <summary>
	/// Name of the Environment variable for initializing the database.
	/// </summary>
	public const string ENV_INIT_DB = "INIT_DB";
}
