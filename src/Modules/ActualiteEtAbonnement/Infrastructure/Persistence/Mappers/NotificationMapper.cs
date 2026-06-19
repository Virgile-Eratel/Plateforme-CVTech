using CVTech.Modules.ActualiteEtAbonnement.Domaine;
using CVTech.Modules.ActualiteEtAbonnement.Infrastructure.Persistence.Entities;

namespace CVTech.Modules.ActualiteEtAbonnement.Infrastructure.Persistence.Mappers;

/// <summary>Conversions entre l'agrégat <see cref="Notification"/> et <see cref="NotificationEntity"/>.</summary>
public static class NotificationMapper
{
    public static NotificationEntity ToEntity(Notification domaine) => new()
    {
        Id = domaine.Id,
        DestinataireId = domaine.DestinataireId,
        Titre = domaine.Titre,
        Message = domaine.Message,
        Canal = domaine.Canal,
        DateCreation = domaine.DateCreation,
        Lu = domaine.Lu
    };

    public static Notification ToDomain(NotificationEntity entite) =>
        Notification.Reconstituer(
            entite.Id, entite.DestinataireId, entite.Titre, entite.Message,
            entite.Canal, entite.DateCreation, entite.Lu);

    /// <summary>Reporte l'état mutable de l'agrégat sur une entité déjà suivie (mise à jour).</summary>
    public static void Appliquer(Notification domaine, NotificationEntity entite)
    {
        entite.Titre = domaine.Titre;
        entite.Message = domaine.Message;
        entite.Canal = domaine.Canal;
        entite.DateCreation = domaine.DateCreation;
        entite.Lu = domaine.Lu;
    }
}
