namespace Bat.Shared.Models;

public sealed class Application : Entity<string>
{
	/// <inheritdoc />
	public override string Id { get; set; } = Guid.NewGuid().ToString();

	/// <summary>
	/// Application's display name.
	/// </summary>
	public string DisplayName { get; set; } = default!;

	/// <summary>
	/// Application's public key in PEM format.
	/// </summary>
	public string? PublicKeyPEM { get; set; }

	public override string ToString() => DisplayName ?? string.Empty;
}

public interface IApplicationRepository : IGenericRepository<Application, string>
{
}
