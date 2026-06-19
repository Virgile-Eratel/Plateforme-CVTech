using CVTech.Modules.CatalogueEmploi.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CVTech.Modules.CatalogueEmploi.Infrastructure.Persistence.Configurations;

public sealed class CurriculumVitaeConfiguration : IEntityTypeConfiguration<CurriculumVitaeEntity>
{
    public void Configure(EntityTypeBuilder<CurriculumVitaeEntity> builder)
    {
        builder.ToTable("Cvs");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.CandidatId).IsRequired();
        builder.Property(c => c.Presentation).IsRequired().HasMaxLength(4000);

        // Liste de compétences : collection primitive stockée en JSON.
        builder.PrimitiveCollection(c => c.Competences).HasColumnName("Competences");
    }
}
