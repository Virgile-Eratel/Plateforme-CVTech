using CVTech.Modules.AppelOffreFreelance.Infrastructure.Persistence.Conventions;
using CVTech.Modules.AppelOffreFreelance.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CVTech.Modules.AppelOffreFreelance.Infrastructure.Persistence;

/// <summary>Contexte EF Core du module Freelance, isolé dans le schéma SQL « freelance » (ADR 0005).</summary>
public sealed class FreelanceDbContext(DbContextOptions<FreelanceDbContext> options) : DbContext(options)
{
    public const string Schema = "freelance";

    public DbSet<AppelOffreEntity> AppelsOffre => Set<AppelOffreEntity>();
    public DbSet<PropositionFreelanceEntity> Propositions => Set<PropositionFreelanceEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FreelanceDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
        ConventionsSqlite.AppliquerSi(this, configurationBuilder);
}
