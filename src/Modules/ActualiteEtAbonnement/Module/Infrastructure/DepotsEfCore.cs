using CVTech.Modules.ActualiteEtAbonnement.Application;
using CVTech.Modules.ActualiteEtAbonnement.Domaine;
using CVTech.Modules.ActualiteEtAbonnement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CVTech.Modules.ActualiteEtAbonnement.Infrastructure;

// Implémentations EF Core / Azure SQL des ports de persistance (ADR 0005).

public sealed class DepotArticlesEfCore(ActualiteDbContext contexte) : IDepotArticles
{
    public async Task AjouterAsync(ArticleActualite article, CancellationToken ct = default) =>
        await contexte.Articles.AddAsync(article, ct);

    public async Task<IReadOnlyList<ArticleActualite>> ListerAsync(
        string? domaineCode = null, CancellationToken ct = default)
    {
        var requete = contexte.Articles.AsQueryable();
        if (!string.IsNullOrWhiteSpace(domaineCode))
            requete = requete.Where(a => a.Domaine != null && a.Domaine.Code == domaineCode);
        return await requete.OrderByDescending(a => a.DatePublication).ToListAsync(ct);
    }

    public Task EnregistrerAsync(CancellationToken ct = default) => contexte.SaveChangesAsync(ct);
}

public sealed class DepotAbonnementsEfCore(ActualiteDbContext contexte) : IDepotAbonnements
{
    public Task<Abonnement?> ObtenirParUtilisateurAsync(Guid utilisateurId, CancellationToken ct = default) =>
        contexte.Abonnements.FirstOrDefaultAsync(a => a.UtilisateurId == utilisateurId, ct);

    public async Task AjouterOuMettreAJourAsync(Abonnement abonnement, CancellationToken ct = default)
    {
        // Si l'agrégat est déjà suivi (chargé dans ce contexte), ses mutations seront persistées
        // au SaveChanges ; sinon c'est un nouvel abonnement à insérer.
        if (contexte.Entry(abonnement).State == EntityState.Detached)
            await contexte.Abonnements.AddAsync(abonnement, ct);
    }

    public async Task<IReadOnlyList<Guid>> ListerAbonnesAuDomaineAsync(
        string domaineCode, CancellationToken ct = default)
    {
        // La collection possédée _domaines est chargée avec chaque agrégat : on filtre via la règle métier.
        var abonnements = await contexte.Abonnements.ToListAsync(ct);
        return abonnements
            .Where(a => a.EstAbonneAu(domaineCode))
            .Select(a => a.UtilisateurId)
            .ToList();
    }

    public Task EnregistrerAsync(CancellationToken ct = default) => contexte.SaveChangesAsync(ct);
}

public sealed class DepotNotificationsEfCore(ActualiteDbContext contexte) : IDepotNotifications
{
    public async Task AjouterAsync(Notification notification, CancellationToken ct = default) =>
        await contexte.Notifications.AddAsync(notification, ct);

    public async Task<IReadOnlyList<Notification>> ListerParDestinataireAsync(
        Guid destinataireId, CancellationToken ct = default) =>
        await contexte.Notifications
            .Where(n => n.DestinataireId == destinataireId)
            .OrderByDescending(n => n.DateCreation)
            .ToListAsync(ct);

    public Task EnregistrerAsync(CancellationToken ct = default) => contexte.SaveChangesAsync(ct);
}
