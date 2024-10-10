namespace Bat.Shared.Identity;
public partial class BatUser
{
	public const string USER_ID_ADMIN = "admin";

	public static readonly BatUser ADMIN = new()
	{
		Id = USER_ID_ADMIN,
		UserName = $"{USER_ID_ADMIN}@local",
		Email = $"{USER_ID_ADMIN}@local",
		Roles = [BatRole.GLOBAL_ADMIN],
		GivenName = "Administrator",
		LastName = "Local",
	};

	public static readonly IEnumerable<BatUser> ALL_BUILTIN_USERS = [ADMIN];
}
