using Bat.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bat.Shared.EF;

public abstract class GenericEntityTypeConfiguration<TEntity, TKey> : IEntityTypeConfiguration<TEntity>
	where TEntity : Entity<TKey>, new()
	where TKey : IEquatable<TKey>
{
	public virtual void Configure(EntityTypeBuilder<TEntity> builder)
	{
		builder.ToTable(typeof(TEntity).Name);
		builder.HasKey(e => e.Id);
		builder.Property(e => e.CreatedAt).ValueGeneratedOnAdd();
		builder.Property(e => e.UpdatedAt).ValueGeneratedOnAddOrUpdate();
		builder.Property(e => e.ConcurrencyStamp).IsConcurrencyToken();
	}
}
