using CVTech.Modules.ActualiteEtAbonnement.Domaine;
using CVTech.SharedKernel.Domaine;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CVTech.Modules.ActualiteEtAbonnement.Infrastructure.Persistence.Configurations;

public sealed class AbonnementConfiguration : IEntityTypeConfiguration<Abonnement>
{
    public void Configure(EntityTypeBuilder<Abonnement> builder)
    {
        builder.ToTable("Abonnements");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.UtilisateurId).IsRequired();
        builder.HasIndex(a => a.UtilisateurId).IsUnique();
        builder.Property(a => a.Canal).HasConversion<string>().HasMaxLength(20).IsRequired();

        // La propriété calculée Domaines est ignorée : on mappe le champ possédé _domaines.
        builder.Ignore(a => a.Domaines);
        builder.OwnsMany<DomaineMetier>("_domaines", d =>
        {
            d.ToTable("AbonnementDomaines");
            d.WithOwner().HasForeignKey("AbonnementId");
            d.Property(x => x.Code).HasColumnName("Code").IsRequired().HasMaxLength(120);
            d.Property(x => x.Libelle).HasColumnName("Libelle").IsRequired().HasMaxLength(120);
            d.HasKey("AbonnementId", "Code");
        });

        builder.Ignore(a => a.EvenementsNonPublies);
    }
}
