using Microsoft.AspNetCore.Components;

namespace Bat.Blazor.Client.Pages;

public partial class Login
{
	[Inject]
	public HttpClient HttpClient { get; set; } = default!;

	//[Inject]
	//public IHttpClientFactory HttpClientFactory { get; set; } = default!;

	protected override async Task OnInitializedAsync()
	{
		Console.WriteLine($"Empty Layout Initialized {HttpClient}");
		await Task.CompletedTask;
	}

	private void ButtonClick()
	{
		Console.WriteLine($"Button clicked {HttpClient}");
	}
}
