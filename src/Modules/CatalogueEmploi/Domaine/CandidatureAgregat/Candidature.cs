using CVTech.SharedKernel.Domaine;

namespace CVTech.Modules.CatalogueEmploi.Domaine;

/// <summary>Agrégat : candidature d'un candidat à une annonce d'emploi.</summary>
public sealed class Candidature : RacineAgregat<Guid>
{
    public Guid AnnonceId { get; private set; }
    public Guid CandidatId { get; private set; }
    public string? LettreMotivation { get; private set; }
    public DateTimeOffset DateDepot { get; private set; }

    private Candidature() { }

    public static Candidature Deposer(Guid annonceId, Guid candidatId, string? lettreMotivation)
    {
        if (annonceId == Guid.Empty)
            throw new ArgumentException("L'annonce est obligatoire.", nameof(annonceId));

        return new Candidature
        {
            Id = Guid.NewGuid(),
            AnnonceId = annonceId,
            CandidatId = candidatId,
            LettreMotivation = string.IsNullOrWhiteSpace(lettreMotivation) ? null : lettreMotivation.Trim(),
            DateDepot = DateTimeOffset.UtcNow
        };
    }

    /// <summary>Réhydrate l'agrégat depuis la persistance (mapper Infrastructure), Id préservé.</summary>
    public static Candidature Reconstituer(
        Guid id, Guid annonceId, Guid candidatId, string? lettreMotivation, DateTimeOffset dateDepot) =>
        new()
        {
            Id = id,
            AnnonceId = annonceId,
            CandidatId = candidatId,
            LettreMotivation = lettreMotivation,
            DateDepot = dateDepot
        };
}
