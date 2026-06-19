using CVTech.Modules.AppelOffreFreelance.Domaine;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CVTech.Modules.AppelOffreFreelance.Infrastructure.Persistence.Configurations;

public sealed class PropositionFreelanceConfiguration : IEntityTypeConfiguration<PropositionFreelance>
{
    public void Configure(EntityTypeBuilder<PropositionFreelance> builder)
    {
        builder.ToTable("Propositions");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.AppelOffreId).IsRequired();
        builder.Property(p => p.CandidatId).IsRequired();
        builder.Property(p => p.DureeJours).IsRequired();
        builder.Property(p => p.Methodologie).HasMaxLength(4000);
        builder.Property(p => p.DateSoumission).IsRequired();

        // Objet de Valeur BaremeTJM (owned type).
        builder.OwnsOne(p => p.Tjm, t =>
            t.Property(x => x.MontantJournalier).HasColumnName("Tjm").HasPrecision(18, 2));
        builder.Navigation(p => p.Tjm).IsRequired();

        builder.Ignore(p => p.EvenementsNonPublies);
    }
}
