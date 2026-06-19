using CVTech.Modules.CatalogueEmploi.Domaine;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CVTech.Modules.CatalogueEmploi.Infrastructure.Persistence.Configurations;

public sealed class AnnonceEmploiConfiguration : IEntityTypeConfiguration<AnnonceEmploi>
{
    public void Configure(EntityTypeBuilder<AnnonceEmploi> builder)
    {
        builder.ToTable("Annonces");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.EntrepriseId).IsRequired();
        builder.Property(a => a.Titre).IsRequired().HasMaxLength(200);
        builder.Property(a => a.Description).HasMaxLength(4000);
        builder.Property(a => a.TypeContrat).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(a => a.DatePublication).IsRequired();

        // Objet de Valeur partagé DomaineMetier : persisté dans la même table (owned type).
        builder.OwnsOne(a => a.Domaine, d =>
        {
            d.Property(x => x.Code).HasColumnName("DomaineCode").IsRequired().HasMaxLength(120);
            d.Property(x => x.Libelle).HasColumnName("DomaineLibelle").IsRequired().HasMaxLength(120);
        });
        builder.Navigation(a => a.Domaine).IsRequired();

        builder.Ignore(a => a.EvenementsNonPublies);
    }
}
