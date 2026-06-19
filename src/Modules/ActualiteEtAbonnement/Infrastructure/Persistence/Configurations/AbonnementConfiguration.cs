using CVTech.Modules.ActualiteEtAbonnement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CVTech.Modules.ActualiteEtAbonnement.Infrastructure.Persistence.Configurations;

public sealed class AbonnementConfiguration : IEntityTypeConfiguration<AbonnementEntity>
{
    public void Configure(EntityTypeBuilder<AbonnementEntity> builder)
    {
        builder.ToTable("Abonnements");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.UtilisateurId).IsRequired();
        builder.HasIndex(a => a.UtilisateurId).IsUnique();
        builder.Property(a => a.Canal).HasConversion<string>().HasMaxLength(20).IsRequired();

        // Collection possédée des domaines suivis (table AbonnementDomaines, schéma préservé).
        builder.OwnsMany(a => a.Domaines, d =>
        {
            d.ToTable("AbonnementDomaines");
            d.WithOwner().HasForeignKey("AbonnementId");
            d.Property(x => x.Code).HasColumnName("Code").IsRequired().HasMaxLength(120);
            d.Property(x => x.Libelle).HasColumnName("Libelle").IsRequired().HasMaxLength(120);
            d.HasKey("AbonnementId", "Code");
        });
    }
}
