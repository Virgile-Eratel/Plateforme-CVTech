using CVTech.Modules.CatalogueEmploi.Domaine;
using CVTech.Modules.CatalogueEmploi.Infrastructure.Persistence.Conventions;
using Microsoft.EntityFrameworkCore;

namespace CVTech.Modules.CatalogueEmploi.Infrastructure.Persistence;

/// <summary>Contexte EF Core du module Emploi, isolé dans le schéma SQL « emploi » (ADR 0005).</summary>
public sealed class EmploiDbContext(DbContextOptions<EmploiDbContext> options) : DbContext(options)
{
    public const string Schema = "emploi";

    public DbSet<AnnonceEmploi> Annonces => Set<AnnonceEmploi>();
    public DbSet<CurriculumVitae> Cvs => Set<CurriculumVitae>();
    public DbSet<Candidature> Candidatures => Set<Candidature>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EmploiDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
        ConventionsSqlite.AppliquerSi(this, configurationBuilder);
}
