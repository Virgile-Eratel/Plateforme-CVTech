using CVTech.Modules.ActualiteEtAbonnement.Domaine;

namespace CVTech.Modules.ActualiteEtAbonnement.Infrastructure.Persistence.Entities;

/// <summary>
/// Entité de persistance EF Core (DAO plat) de l'agrégat <see cref="Notification"/>.
/// Aucune logique métier : simples propriétés mutables mappées en colonnes.
/// </summary>
public sealed class NotificationEntity
{
    public Guid Id { get; set; }
    public Guid DestinataireId { get; set; }
    public string Titre { get; set; } = default!;
    public string Message { get; set; } = default!;
    public CanalDiffusion Canal { get; set; }
    public DateTimeOffset DateCreation { get; set; }
    public bool Lu { get; set; }
}
