using System.Collections.Concurrent;
using CVTech.Modules.ActualiteEtAbonnement.Application;
using CVTech.Modules.ActualiteEtAbonnement.Domaine;

namespace CVTech.Modules.ActualiteEtAbonnement.Infrastructure;

public sealed class DepotArticlesEnMemoire : IDepotArticles
{
    private readonly ConcurrentDictionary<Guid, ArticleActualite> _articles = new();

    public Task AjouterAsync(ArticleActualite article, CancellationToken ct = default)
    {
        _articles[article.Id] = article;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<ArticleActualite>> ListerAsync(string? domaineCode = null, CancellationToken ct = default)
    {
        IEnumerable<ArticleActualite> resultat = _articles.Values.OrderByDescending(a => a.DatePublication);
        if (!string.IsNullOrWhiteSpace(domaineCode))
            resultat = resultat.Where(a => a.Domaine is not null && a.Domaine.Code == domaineCode);
        return Task.FromResult<IReadOnlyList<ArticleActualite>>(resultat.ToList());
    }

    public Task EnregistrerAsync(CancellationToken ct = default) => Task.CompletedTask;
}

public sealed class DepotAbonnementsEnMemoire : IDepotAbonnements
{
    private readonly ConcurrentDictionary<Guid, Abonnement> _parUtilisateur = new();

    public Task<Abonnement?> ObtenirParUtilisateurAsync(Guid utilisateurId, CancellationToken ct = default) =>
        Task.FromResult(_parUtilisateur.GetValueOrDefault(utilisateurId));

    public Task AjouterOuMettreAJourAsync(Abonnement abonnement, CancellationToken ct = default)
    {
        _parUtilisateur[abonnement.UtilisateurId] = abonnement;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Guid>> ListerAbonnesAuDomaineAsync(string domaineCode, CancellationToken ct = default)
    {
        var abonnes = _parUtilisateur.Values
            .Where(a => a.EstAbonneAu(domaineCode))
            .Select(a => a.UtilisateurId)
            .ToList();
        return Task.FromResult<IReadOnlyList<Guid>>(abonnes);
    }

    public Task EnregistrerAsync(CancellationToken ct = default) => Task.CompletedTask;
}

public sealed class DepotNotificationsEnMemoire : IDepotNotifications
{
    private readonly ConcurrentDictionary<Guid, Notification> _notifications = new();

    public Task AjouterAsync(Notification notification, CancellationToken ct = default)
    {
        _notifications[notification.Id] = notification;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Notification>> ListerParDestinataireAsync(Guid destinataireId, CancellationToken ct = default)
    {
        var resultat = _notifications.Values.Where(n => n.DestinataireId == destinataireId).ToList();
        return Task.FromResult<IReadOnlyList<Notification>>(resultat);
    }

    public Task EnregistrerAsync(CancellationToken ct = default) => Task.CompletedTask;
}
