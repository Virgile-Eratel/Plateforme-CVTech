using CVTech.Modules.GestionIdentite.Domaine;
using CVTech.Modules.GestionIdentite.Infrastructure.Persistence;
using CVTech.Modules.GestionIdentite.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace CVTech.Modules.GestionIdentite.Infrastructure;

/// <summary>
/// Implémentation EF Core / Azure SQL du port <see cref="IDepotUtilisateurs"/>.
/// Mappe l'agrégat vers/depuis <c>UtilisateurEntity</c> et persiste directement.
/// </summary>
public sealed class UtilisateurRepository(IdentiteDbContext contexte) : IDepotUtilisateurs
{
    public async Task AjouterAsync(Utilisateur utilisateur, CancellationToken ct = default)
    {
        contexte.Utilisateurs.Add(UtilisateurMapper.ToEntity(utilisateur));
        await contexte.SaveChangesAsync(ct);
    }

    public async Task<Utilisateur?> ObtenirAsync(Guid id, CancellationToken ct = default)
    {
        var entite = await contexte.Utilisateurs.FirstOrDefaultAsync(u => u.Id == id, ct);
        return entite is null ? null : UtilisateurMapper.ToDomain(entite);
    }

    public async Task<Utilisateur?> ObtenirParEmailAsync(string email, CancellationToken ct = default)
    {
        var entite = await contexte.Utilisateurs.FirstOrDefaultAsync(u => u.Email == email, ct);
        return entite is null ? null : UtilisateurMapper.ToDomain(entite);
    }

    public async Task MettreAJourAsync(Utilisateur utilisateur, CancellationToken ct = default)
    {
        var entite = await contexte.Utilisateurs.FirstOrDefaultAsync(u => u.Id == utilisateur.Id, ct);
        if (entite is null) return;

        UtilisateurMapper.Appliquer(utilisateur, entite);
        await contexte.SaveChangesAsync(ct);
    }
}
