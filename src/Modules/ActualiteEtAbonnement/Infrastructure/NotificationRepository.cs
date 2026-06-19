using CVTech.Modules.ActualiteEtAbonnement.Domaine;
using CVTech.Modules.ActualiteEtAbonnement.Infrastructure.Persistence;
using CVTech.Modules.ActualiteEtAbonnement.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace CVTech.Modules.ActualiteEtAbonnement.Infrastructure;

/// <summary>
/// Implémentation EF Core / Azure SQL du port <see cref="IDepotNotifications"/>.
/// Mappe l'agrégat vers/depuis <c>NotificationEntity</c> et persiste directement (ADR 0005).
/// </summary>
public sealed class NotificationRepository(ActualiteDbContext contexte) : IDepotNotifications
{
    public async Task AjouterAsync(Notification notification, CancellationToken ct = default)
    {
        contexte.Notifications.Add(NotificationMapper.ToEntity(notification));
        await contexte.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Notification>> ListerParDestinataireAsync(
        Guid destinataireId, CancellationToken ct = default)
    {
        var entites = await contexte.Notifications
            .Where(n => n.DestinataireId == destinataireId)
            .OrderByDescending(n => n.DateCreation)
            .ToListAsync(ct);
        return entites.Select(NotificationMapper.ToDomain).ToList();
    }

    public async Task MettreAJourAsync(Notification notification, CancellationToken ct = default)
    {
        var entite = await contexte.Notifications.FirstOrDefaultAsync(n => n.Id == notification.Id, ct);
        if (entite is null) return;

        NotificationMapper.Appliquer(notification, entite);
        await contexte.SaveChangesAsync(ct);
    }
}
