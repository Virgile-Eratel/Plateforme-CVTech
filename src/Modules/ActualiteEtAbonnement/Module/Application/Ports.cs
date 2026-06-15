using CVTech.Modules.ActualiteEtAbonnement.Domaine;

namespace CVTech.Modules.ActualiteEtAbonnement.Application;

public interface IDepotArticles
{
    Task AjouterAsync(ArticleActualite article, CancellationToken ct = default);
    Task<IReadOnlyList<ArticleActualite>> ListerAsync(string? domaineCode = null, CancellationToken ct = default);
    Task EnregistrerAsync(CancellationToken ct = default);
}

public interface IDepotAbonnements
{
    Task<Abonnement?> ObtenirParUtilisateurAsync(Guid utilisateurId, CancellationToken ct = default);
    Task AjouterOuMettreAJourAsync(Abonnement abonnement, CancellationToken ct = default);
    Task<IReadOnlyList<Guid>> ListerAbonnesAuDomaineAsync(string domaineCode, CancellationToken ct = default);
    Task EnregistrerAsync(CancellationToken ct = default);
}

public interface IDepotNotifications
{
    Task AjouterAsync(Notification notification, CancellationToken ct = default);
    Task<IReadOnlyList<Notification>> ListerParDestinataireAsync(Guid destinataireId, CancellationToken ct = default);
    Task EnregistrerAsync(CancellationToken ct = default);
}

/// <summary>Port de diffusion temps réel (implémenté par SignalR dans Infrastructure).</summary>
public interface INotificateurTempsReel
{
    Task PousserAsync(Guid destinataireId, Notification notification, CancellationToken ct = default);
}

/// <summary>Port de génération du flux RSS 2.0 (implémenté dans Infrastructure).</summary>
public interface IGenerateurRss
{
    string Generer(IReadOnlyList<ArticleActualite> articles, string? domaineCode);
}
