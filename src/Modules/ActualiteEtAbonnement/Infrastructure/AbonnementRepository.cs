using CVTech.Modules.ActualiteEtAbonnement.Domaine;
using CVTech.Modules.ActualiteEtAbonnement.Infrastructure.Persistence;
using CVTech.Modules.ActualiteEtAbonnement.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace CVTech.Modules.ActualiteEtAbonnement.Infrastructure;

/// <summary>
/// Implémentation EF Core / Azure SQL du port <see cref="IDepotAbonnements"/>.
/// Mappe l'agrégat vers/depuis <c>AbonnementEntity</c> et persiste directement (ADR 0005).
/// </summary>
public sealed class AbonnementRepository(ActualiteDbContext contexte) : IDepotAbonnements
{
    public async Task<Abonnement?> ObtenirParUtilisateurAsync(Guid utilisateurId, CancellationToken ct = default)
    {
        var entite = await contexte.Abonnements
            .Include(a => a.Domaines)
            .FirstOrDefaultAsync(a => a.UtilisateurId == utilisateurId, ct);
        return entite is null ? null : AbonnementMapper.ToDomain(entite);
    }

    /// <summary>
    /// Upsert sur l'abonnement d'un utilisateur : avec le mapper l'agrégat est toujours détaché,
    /// on recharge donc l'entité par <c>UtilisateurId</c> pour décider insertion ou mise à jour.
    /// </summary>
    public async Task AjouterOuMettreAJourAsync(Abonnement abonnement, CancellationToken ct = default)
    {
        var entite = await contexte.Abonnements
            .Include(a => a.Domaines)
            .FirstOrDefaultAsync(a => a.UtilisateurId == abonnement.UtilisateurId, ct);

        if (entite is null)
            contexte.Abonnements.Add(AbonnementMapper.ToEntity(abonnement));
        else
            AbonnementMapper.Appliquer(abonnement, entite);

        await contexte.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Guid>> ListerAbonnesAuDomaineAsync(
        string domaineCode, CancellationToken ct = default)
    {
        // Filtre directement sur la table possédée des domaines suivis.
        return await contexte.Abonnements
            .Where(a => a.Domaines.Any(d => d.Code == domaineCode))
            .Select(a => a.UtilisateurId)
            .ToListAsync(ct);
    }
}
