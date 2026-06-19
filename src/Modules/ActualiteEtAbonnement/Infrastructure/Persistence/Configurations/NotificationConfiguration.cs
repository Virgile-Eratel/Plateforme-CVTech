using CVTech.Modules.ActualiteEtAbonnement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CVTech.Modules.ActualiteEtAbonnement.Infrastructure.Persistence.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<NotificationEntity>
{
    public void Configure(EntityTypeBuilder<NotificationEntity> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).ValueGeneratedNever();

        builder.Property(n => n.DestinataireId).IsRequired();
        builder.HasIndex(n => n.DestinataireId);
        builder.Property(n => n.Titre).IsRequired().HasMaxLength(200);
        builder.Property(n => n.Message).IsRequired().HasMaxLength(2000);
        builder.Property(n => n.Canal).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(n => n.DateCreation).IsRequired();
        builder.Property(n => n.Lu).IsRequired();
    }
}
