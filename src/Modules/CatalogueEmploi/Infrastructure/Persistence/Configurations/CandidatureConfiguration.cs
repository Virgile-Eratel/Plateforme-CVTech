using CVTech.Modules.CatalogueEmploi.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CVTech.Modules.CatalogueEmploi.Infrastructure.Persistence.Configurations;

public sealed class CandidatureConfiguration : IEntityTypeConfiguration<CandidatureEntity>
{
    public void Configure(EntityTypeBuilder<CandidatureEntity> builder)
    {
        builder.ToTable("Candidatures");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.AnnonceId).IsRequired();
        builder.Property(c => c.CandidatId).IsRequired();
        builder.Property(c => c.LettreMotivation).HasMaxLength(4000);
        builder.Property(c => c.DateDepot).IsRequired();
    }
}
