using CVTech.Modules.AppelOffreFreelance.Domaine;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CVTech.Modules.AppelOffreFreelance.Infrastructure.Persistence.Configurations;

public sealed class AppelOffreConfiguration : IEntityTypeConfiguration<AppelOffre>
{
    public void Configure(EntityTypeBuilder<AppelOffre> builder)
    {
        builder.ToTable("AppelsOffre");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.EntrepriseId).IsRequired();
        builder.Property(a => a.Titre).IsRequired().HasMaxLength(200);
        builder.Property(a => a.Statut).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(a => a.PropositionLaureateId);
        builder.Property(a => a.DatePublication).IsRequired();

        // Objet de Valeur CahierDesCharges (owned type).
        builder.OwnsOne(a => a.CahierDesCharges, c =>
        {
            c.Property(x => x.Contexte).HasColumnName("Contexte").IsRequired().HasMaxLength(4000);
            c.Property(x => x.Livrables).HasColumnName("Livrables").IsRequired().HasMaxLength(4000);
            c.Property(x => x.Deadline).HasColumnName("Deadline").IsRequired();
            c.Property(x => x.BudgetMin).HasColumnName("BudgetMin").HasPrecision(18, 2);
            c.Property(x => x.BudgetMax).HasColumnName("BudgetMax").HasPrecision(18, 2);
        });
        builder.Navigation(a => a.CahierDesCharges).IsRequired();

        // Objet de Valeur partagé DomaineMetier (owned type).
        builder.OwnsOne(a => a.Domaine, d =>
        {
            d.Property(x => x.Code).HasColumnName("DomaineCode").IsRequired().HasMaxLength(120);
            d.Property(x => x.Libelle).HasColumnName("DomaineLibelle").IsRequired().HasMaxLength(120);
        });
        builder.Navigation(a => a.Domaine).IsRequired();

        builder.Ignore(a => a.EvenementsNonPublies);
    }
}
