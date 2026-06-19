using CVTech.SharedKernel.Domaine;

namespace CVTech.Modules.AppelOffreFreelance.Domaine;

/// <summary>Agrégat : proposition chiffrée d'un candidat indépendant à un appel d'offre.</summary>
public sealed class PropositionFreelance : RacineAgregat<Guid>
{
    public Guid AppelOffreId { get; private set; }
    public Guid CandidatId { get; private set; }
    public BaremeTJM Tjm { get; private set; } = default!;
    public int DureeJours { get; private set; }
    public string Methodologie { get; private set; } = default!;
    public DateTimeOffset DateSoumission { get; private set; }

    private PropositionFreelance() { }

    /// <summary>Réhydrate l'agrégat depuis la persistance (mapper Infrastructure), sans revalider les invariants.</summary>
    public static PropositionFreelance Reconstituer(
        Guid id,
        Guid appelOffreId,
        Guid candidatId,
        BaremeTJM tjm,
        int dureeJours,
        string methodologie,
        DateTimeOffset dateSoumission) =>
        new()
        {
            Id = id,
            AppelOffreId = appelOffreId,
            CandidatId = candidatId,
            Tjm = tjm,
            DureeJours = dureeJours,
            Methodologie = methodologie,
            DateSoumission = dateSoumission
        };

    public static PropositionFreelance Soumettre(
        Guid appelOffreId, Guid candidatId, BaremeTJM tjm, int dureeJours, string methodologie)
    {
        if (appelOffreId == Guid.Empty)
            throw new ArgumentException("L'appel d'offre est obligatoire.", nameof(appelOffreId));
        if (dureeJours <= 0)
            throw new ArgumentException("La durée doit être strictement positive.", nameof(dureeJours));

        return new PropositionFreelance
        {
            Id = Guid.NewGuid(),
            AppelOffreId = appelOffreId,
            CandidatId = candidatId,
            Tjm = tjm,
            DureeJours = dureeJours,
            Methodologie = methodologie?.Trim() ?? string.Empty,
            DateSoumission = DateTimeOffset.UtcNow
        };
    }
}
