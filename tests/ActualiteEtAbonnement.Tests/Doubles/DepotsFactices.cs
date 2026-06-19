using CVTech.Modules.ActualiteEtAbonnement.Application;
using CVTech.Modules.ActualiteEtAbonnement.Domaine;

namespace CVTech.Modules.ActualiteEtAbonnement.Tests.Doubles;

/// <summary>Faux dépôt d'abonnements (test unitaire du handler, sans base réelle).</summary>
public sealed class DepotAbonnementsFactice : IDepotAbonnements
{
    private readonly Dictionary<Guid, Abonnement> _parUtilisateur = new();

    public Task<Abonnement?> ObtenirParUtilisateurAsync(Guid utilisateurId, CancellationToken ct = default) =>
        Task.FromResult(_parUtilisateur.GetValueOrDefault(utilisateurId));

    public Task AjouterOuMettreAJourAsync(Abonnement abonnement, CancellationToken ct = default)
    {
        // Sémantique upsert : un abonnement par utilisateur.
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
}

/// <summary>Faux dépôt de notifications (test unitaire du handler, sans base réelle).</summary>
public sealed class DepotNotificationsFactice : IDepotNotifications
{
    private readonly List<Notification> _notifications = [];

    public Task AjouterAsync(Notification notification, CancellationToken ct = default)
    {
        _notifications.Add(notification);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Notification>> ListerParDestinataireAsync(Guid destinataireId, CancellationToken ct = default)
    {
        var resultat = _notifications.Where(n => n.DestinataireId == destinataireId).ToList();
        return Task.FromResult<IReadOnlyList<Notification>>(resultat);
    }

    public Task MettreAJourAsync(Notification notification, CancellationToken ct = default)
    {
        var index = _notifications.FindIndex(n => n.Id == notification.Id);
        if (index >= 0) _notifications[index] = notification;
        return Task.CompletedTask;
    }
}
