using CVTech.Modules.ActualiteEtAbonnement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CVTech.Modules.ActualiteEtAbonnement.Infrastructure.Persistence.Configurations;

public sealed class ArticleActualiteConfiguration : IEntityTypeConfiguration<ArticleActualiteEntity>
{
    public void Configure(EntityTypeBuilder<ArticleActualiteEntity> builder)
    {
        builder.ToTable("Articles");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.AuteurId).IsRequired();
        builder.Property(a => a.Titre).IsRequired().HasMaxLength(200);
        builder.Property(a => a.Contenu).IsRequired();
        builder.Property(a => a.Categorie).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(a => a.DatePublication).IsRequired();

        // Domaine métier optionnel : colonnes plates nullables (un article peut n'en cibler aucun).
        builder.Property(a => a.DomaineCode).HasColumnName("DomaineCode").HasMaxLength(120);
        builder.Property(a => a.DomaineLibelle).HasColumnName("DomaineLibelle").HasMaxLength(120);

        // Source externe optionnelle : colonnes plates nullables.
        builder.Property(a => a.SourceNom).HasColumnName("SourceNom").HasMaxLength(200);
        builder.Property(a => a.SourceUrl).HasColumnName("SourceUrl").HasMaxLength(500);
    }
}
