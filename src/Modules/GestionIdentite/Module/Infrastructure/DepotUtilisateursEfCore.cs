using CVTech.Modules.GestionIdentite.Application;
using CVTech.Modules.GestionIdentite.Domaine;
using CVTech.Modules.GestionIdentite.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CVTech.Modules.GestionIdentite.Infrastructure;

/// <summary>
/// Implémentation EF Core / Azure SQL du port <see cref="IDepotUtilisateurs"/> (ADR 0005).
/// Remplace le dépôt en mémoire sans changer le contrat consommé par l'Application.
/// </summary>
public sealed class DepotUtilisateursEfCore(IdentiteDbContext contexte) : IDepotUtilisateurs
{
    public async Task AjouterAsync(Utilisateur utilisateur, CancellationToken ct = default) =>
        await contexte.Utilisateurs.AddAsync(utilisateur, ct);

    public Task<Utilisateur?> ObtenirAsync(Guid id, CancellationToken ct = default) =>
        contexte.Utilisateurs.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task EnregistrerAsync(CancellationToken ct = default) => contexte.SaveChangesAsync(ct);
}
