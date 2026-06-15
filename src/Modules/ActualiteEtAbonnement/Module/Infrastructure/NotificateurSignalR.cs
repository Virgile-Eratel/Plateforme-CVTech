using CVTech.Modules.ActualiteEtAbonnement.Application;
using CVTech.Modules.ActualiteEtAbonnement.Client;
using CVTech.Modules.ActualiteEtAbonnement.Domaine;
using Microsoft.AspNetCore.SignalR;

namespace CVTech.Modules.ActualiteEtAbonnement.Infrastructure;

/// <summary>Implémentation in-app du port de notification temps réel via SignalR.</summary>
public sealed class NotificateurSignalR(IHubContext<NotificationsHub> hub) : INotificateurTempsReel
{
    public Task PousserAsync(Guid destinataireId, Notification notification, CancellationToken ct = default) =>
        hub.Clients.Group(destinataireId.ToString()).SendAsync(
            "RecevoirNotification",
            new
            {
                notification.Id,
                notification.Titre,
                notification.Message,
                notification.DateCreation
            },
            ct);
}
