using CVTech.Modules.CatalogueEmploi.Infrastructure.Persistence.Conventions;
using CVTech.Modules.CatalogueEmploi.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CVTech.Modules.CatalogueEmploi.Infrastructure.Persistence;

/// <summary>Contexte EF Core du module Emploi, isolé dans le schéma SQL « emploi » (ADR 0005).</summary>
public sealed class EmploiDbContext(DbContextOptions<EmploiDbContext> options) : DbContext(options)
{
    public const string Schema = "emploi";

    public DbSet<AnnonceEmploiEntity> Annonces => Set<AnnonceEmploiEntity>();
    public DbSet<CurriculumVitaeEntity> Cvs => Set<CurriculumVitaeEntity>();
    public DbSet<CandidatureEntity> Candidatures => Set<CandidatureEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EmploiDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
        ConventionsSqlite.AppliquerSi(this, configurationBuilder);
}
