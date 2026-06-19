using CVTech.Modules.ActualiteEtAbonnement.Domaine;
using CVTech.Modules.AppelOffreFreelance.Contracts;
using CVTech.Modules.CatalogueEmploi.Contracts;
using MediatR;

namespace CVTech.Modules.ActualiteEtAbonnement.Application.EventHandlers;

/// <summary>
/// Logique commune : pour un domaine donné, crée une notification in-app pour CHAQUE
/// abonné concerné (et uniquement eux), la persiste et la pousse en temps réel.
/// </summary>
internal static class DiffuseurNotifications
{
    public static async Task DiffuserAsync(
        IDepotAbonnements abonnements,
        IDepotNotifications notifications,
        INotificateurTempsReel notificateur,
        string domaineCode,
        string titre,
        string message,
        CancellationToken ct)
    {
        var abonnes = await abonnements.ListerAbonnesAuDomaineAsync(domaineCode, ct);
        foreach (var destinataireId in abonnes)
        {
            var notification = Notification.Creer(destinataireId, titre, message, CanalDiffusion.InApp);
            await notifications.AjouterAsync(notification, ct);
            await notificateur.PousserAsync(destinataireId, notification, ct);
        }
    }
}

/// <summary>Réagit à l'événement AnnoncePubliee émis par CatalogueEmploi (bus interne).</summary>
public sealed class NotifierSurAnnoncePubliee(
    IDepotAbonnements abonnements,
    IDepotNotifications notifications,
    INotificateurTempsReel notificateur) : INotificationHandler<AnnoncePubliee>
{
    public Task Handle(AnnoncePubliee e, CancellationToken ct) =>
        DiffuseurNotifications.DiffuserAsync(
            abonnements, notifications, notificateur, e.DomaineCode,
            "Nouvelle annonce d'emploi",
            $"« {e.Titre} » vient d'être publiée dans le domaine {e.DomaineLibelle}.",
            ct);
}

/// <summary>Réagit à l'événement AppelOffrePublie émis par AppelOffreFreelance (bus interne).</summary>
public sealed class NotifierSurAppelOffrePublie(
    IDepotAbonnements abonnements,
    IDepotNotifications notifications,
    INotificateurTempsReel notificateur) : INotificationHandler<AppelOffrePublie>
{
    public Task Handle(AppelOffrePublie e, CancellationToken ct) =>
        DiffuseurNotifications.DiffuserAsync(
            abonnements, notifications, notificateur, e.DomaineCode,
            "Nouvel appel d'offre",
            $"« {e.Titre} » vient d'être publié dans le domaine {e.DomaineLibelle}.",
            ct);
}
