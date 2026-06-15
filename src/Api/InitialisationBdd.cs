using CVTech.Modules.ActualiteEtAbonnement.Infrastructure.Persistence;
using CVTech.Modules.AppelOffreFreelance.Infrastructure.Persistence;
using CVTech.Modules.CatalogueEmploi.Infrastructure.Persistence;
using CVTech.Modules.GestionIdentite.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CVTech.Api;

/// <summary>
/// Préparation des schémas de chaque module au démarrage.
/// - Azure SQL (SqlServer) : applique les migrations EF Core.
/// - SQLite (local/dev) : crée le schéma à partir du modèle (EnsureCreated, sans migrations).
/// </summary>
public static class InitialisationBdd
{
    public static async Task InitialiserBaseDeDonneesAsync(this WebApplication app, string fournisseur)
    {
        using var scope = app.Services.CreateScope();
        var sp = scope.ServiceProvider;

        DbContext[] contextes =
        [
            sp.GetRequiredService<IdentiteDbContext>(),
            sp.GetRequiredService<EmploiDbContext>(),
            sp.GetRequiredService<FreelanceDbContext>(),
            sp.GetRequiredService<ActualiteDbContext>(),
        ];

        var surSqlServer = fournisseur.Equals("SqlServer", StringComparison.OrdinalIgnoreCase);
        foreach (var contexte in contextes)
        {
            if (surSqlServer)
                await contexte.Database.MigrateAsync();
            else
                await contexte.Database.EnsureCreatedAsync();
        }
    }
}
