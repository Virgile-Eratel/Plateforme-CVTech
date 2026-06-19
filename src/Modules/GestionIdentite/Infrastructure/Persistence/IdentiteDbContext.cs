using CVTech.Modules.GestionIdentite.Domaine;
using Microsoft.EntityFrameworkCore;

namespace CVTech.Modules.GestionIdentite.Infrastructure.Persistence;

/// <summary>
/// Contexte EF Core du module Identité. Isolé dans son propre schéma SQL « identite »
/// (ADR 0005 : un schéma par module). La persistance vit exclusivement ici, jamais dans le Domaine.
/// </summary>
public sealed class IdentiteDbContext(DbContextOptions<IdentiteDbContext> options) : DbContext(options)
{
    public const string Schema = "identite";

    public DbSet<Utilisateur> Utilisateurs => Set<Utilisateur>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentiteDbContext).Assembly);
    }
}
