namespace Bat.Shared.Models;

public abstract class Entity<TKey> where TKey : IEquatable<TKey>
{
	/// <summary>
	/// The entity's ID.
	/// </summary>
	public virtual TKey Id { get; set; } = default!;

	/// <summary>
	/// The entity's creation date.
	/// </summary>
	public virtual DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

	/// <summary>
	/// The entity's last update date.
	/// </summary>
	public virtual DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

	/// <summary>
	/// A random value that must change whenever the entity is persisted to the store.
	/// </summary>
	public virtual string? ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();

	/// <summary>
	/// Touches the entity, updating the <see cref="UpdatedAt"/> and <see cref="ConcurrencyStamp"/> properties.
	/// </summary>
	public virtual void Touch()
	{
		UpdatedAt = DateTimeOffset.UtcNow;
		ConcurrencyStamp = Guid.NewGuid().ToString();
	}

	/// <inheritdoc/>
	public override bool Equals(object? obj) => obj is Entity<TKey> other
		&& (ReferenceEquals(this, other) || EqualityComparer<TKey>.Default.Equals(Id, other.Id));

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		return Id.GetHashCode();
	}

	/// <summary>
	/// Clones the entity.
	/// </summary>
	public virtual Entity<TKey> Clone()
	{
		return (Entity<TKey>)MemberwiseClone();
	}
}
