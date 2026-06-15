using CVTech.Modules.ActualiteEtAbonnement.Domaine;
using CVTech.Modules.ActualiteEtAbonnement.Infrastructure.Persistence.Conventions;
using Microsoft.EntityFrameworkCore;

namespace CVTech.Modules.ActualiteEtAbonnement.Infrastructure.Persistence;

/// <summary>Contexte EF Core du module Actualité, isolé dans le schéma SQL « actualite » (ADR 0005).</summary>
public sealed class ActualiteDbContext(DbContextOptions<ActualiteDbContext> options) : DbContext(options)
{
    public const string Schema = "actualite";

    public DbSet<ArticleActualite> Articles => Set<ArticleActualite>();
    public DbSet<Abonnement> Abonnements => Set<Abonnement>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ActualiteDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
        ConventionsSqlite.AppliquerSi(this, configurationBuilder);
}
