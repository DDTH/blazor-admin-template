namespace Bat.Shared.Api;
public interface IApiClient
{
	/// <summary>
	/// Calls the <c>/info</c> API.
	/// </summary>
	/// <returns></returns>
	public Task<ApiResp<InfoResp>> InfoAsync();
}
