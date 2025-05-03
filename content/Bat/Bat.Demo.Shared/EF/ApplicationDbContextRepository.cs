using Bat.Demo.Shared.Models;
using Bat.Shared.Cache;
using Bat.Shared.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bat.Demo.Shared.EF;

public sealed class ApplicationDbContextRepository
	: CacheSupportedGenericDbContextRepository<ApplicationDbContextRepository, Application, string>, IApplicationRepository
{
	public ApplicationDbContextRepository(
		DbContextOptions<ApplicationDbContextRepository> options,
		ICacheFacade<Application>? cache = default)
		: this(options, cache, default) { }

	public ApplicationDbContextRepository(
		DbContextOptions<ApplicationDbContextRepository> options,
		ICacheFacade<Application>? cache = default,
		IEntityTypeConfiguration<Application>? entityTypeConfiguration = default)
		: base(options, entityTypeConfiguration ?? new ApplicationEntityTypeConfiguration(), cache) { }
}

sealed class ApplicationEntityTypeConfiguration : GenericEntityTypeConfiguration<Application, string>
{
	public override void Configure(EntityTypeBuilder<Application> builder)
	{
		base.Configure(builder);
		builder.ToTable($"{Globals.TABLE_PREFIX}apps"); // change table name if needed
		builder.Property(t => t.Id).HasColumnName("app_id").HasMaxLength(64);
		builder.Property(builder => builder.DisplayName).HasColumnName("display_name").HasMaxLength(128);
		builder.Property(builder => builder.PublicKeyPEM).HasColumnName("public_key_pem");
		builder.Property(builder => builder.CreatedAt).HasColumnName("created_at");
		builder.Property(builder => builder.UpdatedAt).HasColumnName("updated_at");
		builder.Property(builder => builder.ConcurrencyStamp).HasColumnName("concurrency_stamp").HasMaxLength(64);
	}
}
