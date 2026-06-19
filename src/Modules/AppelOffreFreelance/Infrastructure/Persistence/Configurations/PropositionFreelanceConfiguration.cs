using CVTech.Modules.AppelOffreFreelance.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CVTech.Modules.AppelOffreFreelance.Infrastructure.Persistence.Configurations;

/// <summary>Mapping relationnel de l'entité de persistance <see cref="PropositionFreelanceEntity"/>.</summary>
public sealed class PropositionFreelanceConfiguration : IEntityTypeConfiguration<PropositionFreelanceEntity>
{
    public void Configure(EntityTypeBuilder<PropositionFreelanceEntity> builder)
    {
        builder.ToTable("Propositions");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.AppelOffreId).IsRequired();
        builder.Property(p => p.CandidatId).IsRequired();
        builder.Property(p => p.DureeJours).IsRequired();
        builder.Property(p => p.Methodologie).HasMaxLength(4000);
        builder.Property(p => p.DateSoumission).IsRequired();

        // Value object BaremeTJM (aplati en colonne).
        builder.Property(p => p.Tjm).HasColumnName("Tjm").HasPrecision(18, 2);
    }
}
