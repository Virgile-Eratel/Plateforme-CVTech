namespace CVTech.Modules.ActualiteEtAbonnement.Domaine;

/// <summary>Port de persistance de l'agrégat Notification (implémenté dans Infrastructure).</summary>
public interface IDepotNotifications
{
    Task AjouterAsync(Notification notification, CancellationToken ct = default);
    Task<IReadOnlyList<Notification>> ListerParDestinataireAsync(Guid destinataireId, CancellationToken ct = default);
    Task MettreAJourAsync(Notification notification, CancellationToken ct = default);
}
