using MediatR;

using CVTech.Modules.ActualiteEtAbonnement.Domaine;

namespace CVTech.Modules.ActualiteEtAbonnement.Application.Features.ListerNotifications;

/// <summary>Liste les notifications in-app d'un utilisateur (les siennes).</summary>
public sealed record ListerNotificationsQuery(Guid UtilisateurId) : IRequest<IReadOnlyList<NotificationVue>>;

public sealed record NotificationVue(Guid Id, string Titre, string Message, bool Lu, DateTimeOffset DateCreation);

public sealed class ListerNotificationsHandler(IDepotNotifications depot)
    : IRequestHandler<ListerNotificationsQuery, IReadOnlyList<NotificationVue>>
{
    public async Task<IReadOnlyList<NotificationVue>> Handle(ListerNotificationsQuery requete, CancellationToken ct)
    {
        var notifications = await depot.ListerParDestinataireAsync(requete.UtilisateurId, ct);
        return notifications
            .OrderByDescending(n => n.DateCreation)
            .Select(n => new NotificationVue(n.Id, n.Titre, n.Message, n.Lu, n.DateCreation))
            .ToList();
    }
}
