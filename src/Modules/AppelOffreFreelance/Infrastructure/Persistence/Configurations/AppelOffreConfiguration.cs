using CVTech.Modules.AppelOffreFreelance.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CVTech.Modules.AppelOffreFreelance.Infrastructure.Persistence.Configurations;

/// <summary>Mapping relationnel de l'entité de persistance <see cref="AppelOffreEntity"/>.</summary>
public sealed class AppelOffreConfiguration : IEntityTypeConfiguration<AppelOffreEntity>
{
    public void Configure(EntityTypeBuilder<AppelOffreEntity> builder)
    {
        builder.ToTable("AppelsOffre");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.EntrepriseId).IsRequired();
        builder.Property(a => a.Titre).IsRequired().HasMaxLength(200);
        builder.Property(a => a.Statut).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(a => a.PropositionLaureateId);
        builder.Property(a => a.DatePublication).IsRequired();

        // Value object CahierDesCharges (aplati en colonnes).
        builder.Property(a => a.Contexte).HasColumnName("Contexte").IsRequired().HasMaxLength(4000);
        builder.Property(a => a.Livrables).HasColumnName("Livrables").IsRequired().HasMaxLength(4000);
        builder.Property(a => a.Deadline).HasColumnName("Deadline").IsRequired();
        builder.Property(a => a.BudgetMin).HasColumnName("BudgetMin").HasPrecision(18, 2);
        builder.Property(a => a.BudgetMax).HasColumnName("BudgetMax").HasPrecision(18, 2);

        // Value object partagé DomaineMetier (aplati en colonnes).
        builder.Property(a => a.DomaineCode).HasColumnName("DomaineCode").IsRequired().HasMaxLength(120);
        builder.Property(a => a.DomaineLibelle).HasColumnName("DomaineLibelle").IsRequired().HasMaxLength(120);
    }
}
