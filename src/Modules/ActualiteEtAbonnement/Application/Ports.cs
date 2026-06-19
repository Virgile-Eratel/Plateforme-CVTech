using CVTech.Modules.ActualiteEtAbonnement.Domaine;

namespace CVTech.Modules.ActualiteEtAbonnement.Application;

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
