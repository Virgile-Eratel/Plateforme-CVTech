using CVTech.Modules.CatalogueEmploi.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CVTech.Modules.CatalogueEmploi.Infrastructure.Persistence.Configurations;

public sealed class AnnonceEmploiConfiguration : IEntityTypeConfiguration<AnnonceEmploiEntity>
{
    public void Configure(EntityTypeBuilder<AnnonceEmploiEntity> builder)
    {
        builder.ToTable("Annonces");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.EntrepriseId).IsRequired();
        builder.Property(a => a.Titre).IsRequired().HasMaxLength(200);
        builder.Property(a => a.Description).HasMaxLength(4000);
        builder.Property(a => a.TypeContrat).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(a => a.DatePublication).IsRequired();

        // VO partagé DomaineMetier aplati en deux colonnes plates.
        builder.Property(a => a.DomaineCode).HasColumnName("DomaineCode").IsRequired().HasMaxLength(120);
        builder.Property(a => a.DomaineLibelle).HasColumnName("DomaineLibelle").IsRequired().HasMaxLength(120);
    }
}
