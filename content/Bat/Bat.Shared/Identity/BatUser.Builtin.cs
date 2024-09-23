namespace Bat.Shared.Identity;
public partial class BatUser
{
	public const string USER_ID_ADMIN = "admin";
	public const string USER_NAME_ADMIN = "admin@local";
	public const string USER_EMAIL_ADMIN = "admin@local";

	public static readonly BatUser ADMIN = new()
	{
		Id = USER_ID_ADMIN,
		UserName = USER_NAME_ADMIN,
		Email = USER_EMAIL_ADMIN,
		EmailConfirmed = true,
		Roles = [BatRole.GLOBAL_ADMIN],
	};

	public static readonly IEnumerable<BatUser> ALL_BUILTIN_USERS = [ADMIN];
}
