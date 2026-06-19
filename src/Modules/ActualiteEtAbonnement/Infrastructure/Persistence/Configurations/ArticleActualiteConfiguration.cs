using CVTech.Modules.ActualiteEtAbonnement.Domaine;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CVTech.Modules.ActualiteEtAbonnement.Infrastructure.Persistence.Configurations;

public sealed class ArticleActualiteConfiguration : IEntityTypeConfiguration<ArticleActualite>
{
    public void Configure(EntityTypeBuilder<ArticleActualite> builder)
    {
        builder.ToTable("Articles");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.AuteurId).IsRequired();
        builder.Property(a => a.Titre).IsRequired().HasMaxLength(200);
        builder.Property(a => a.Contenu).IsRequired();
        builder.Property(a => a.Categorie).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(a => a.DatePublication).IsRequired();

        // Domaine métier : owned type OPTIONNEL (un article éditorial peut n'en cibler aucun).
        builder.OwnsOne(a => a.Domaine, d =>
        {
            d.Property(x => x.Code).HasColumnName("DomaineCode").HasMaxLength(120);
            d.Property(x => x.Libelle).HasColumnName("DomaineLibelle").HasMaxLength(120);
        });

        // Source externe : owned type OPTIONNEL.
        builder.OwnsOne(a => a.Source, s =>
        {
            s.Property(x => x.Nom).HasColumnName("SourceNom").HasMaxLength(200);
            s.Property(x => x.Url).HasColumnName("SourceUrl").HasMaxLength(500);
        });

        builder.Ignore(a => a.EvenementsNonPublies);
    }
}
