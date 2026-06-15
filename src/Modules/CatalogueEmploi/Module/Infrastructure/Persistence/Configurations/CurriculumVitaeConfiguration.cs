using CVTech.Modules.CatalogueEmploi.Domaine;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CVTech.Modules.CatalogueEmploi.Infrastructure.Persistence.Configurations;

public sealed class CurriculumVitaeConfiguration : IEntityTypeConfiguration<CurriculumVitae>
{
    public void Configure(EntityTypeBuilder<CurriculumVitae> builder)
    {
        builder.ToTable("Cvs");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.CandidatId).IsRequired();
        builder.Property(c => c.Presentation).IsRequired().HasMaxLength(4000);

        // Liste de compétences : collection primitive stockée en JSON (sur le champ privé _competences).
        builder.Ignore(c => c.Competences);
        builder.PrimitiveCollection<List<string>>("_competences").HasColumnName("Competences");

        builder.Ignore(c => c.EvenementsNonPublies);
    }
}
