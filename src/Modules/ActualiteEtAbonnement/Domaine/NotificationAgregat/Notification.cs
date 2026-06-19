using CVTech.SharedKernel.Domaine;

namespace CVTech.Modules.ActualiteEtAbonnement.Domaine;

/// <summary>Agrégat : notification adressée à un abonné pour un domaine suivi.</summary>
public sealed class Notification : RacineAgregat<Guid>
{
    public Guid DestinataireId { get; private set; }
    public string Titre { get; private set; } = default!;
    public string Message { get; private set; } = default!;
    public CanalDiffusion Canal { get; private set; }
    public DateTimeOffset DateCreation { get; private set; }
    public bool Lu { get; private set; }

    private Notification() { }

    public static Notification Creer(Guid destinataireId, string titre, string message, CanalDiffusion canal)
    {
        return new Notification
        {
            Id = Guid.NewGuid(),
            DestinataireId = destinataireId,
            Titre = titre,
            Message = message,
            Canal = canal,
            DateCreation = DateTimeOffset.UtcNow,
            Lu = false
        };
    }

    public void MarquerLu() => Lu = true;
}
