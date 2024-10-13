using Bat.Shared.Identity;
using Microsoft.AspNetCore.Authorization;
namespace Bat.Api.Controllers;

[Authorize(Policy = BuiltinPolicies.POLICY_NAME_ADMIN_ROLE_OR_APPLICATION_MANAGER)]
public partial class AppsController : ApiBaseController
{
}
