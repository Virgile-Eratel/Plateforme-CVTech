using CVTech.SharedKernel.Domaine;

namespace CVTech.Modules.CatalogueEmploi.Domaine;

/// <summary>Agrégat : CV structuré d'un candidat, réutilisable pour ses candidatures.</summary>
public sealed class CurriculumVitae : RacineAgregat<Guid>
{
    private readonly List<string> _competences = [];

    public Guid CandidatId { get; private set; }
    public string Presentation { get; private set; } = default!;
    public IReadOnlyCollection<string> Competences => _competences.AsReadOnly();

    private CurriculumVitae() { }

    public static CurriculumVitae Constituer(
        Guid candidatId, string presentation, IEnumerable<string> competences)
    {
        var cv = new CurriculumVitae { Id = Guid.NewGuid(), CandidatId = candidatId };
        cv.Appliquer(presentation, competences);
        return cv;
    }

    /// <summary>Met à jour le CV existant (un seul CV par candidat, on le révise au lieu d'en recréer).</summary>
    public void MettreAJour(string presentation, IEnumerable<string> competences) =>
        Appliquer(presentation, competences);

    private void Appliquer(string presentation, IEnumerable<string> competences)
    {
        if (string.IsNullOrWhiteSpace(presentation))
            throw new ArgumentException("La présentation du CV est obligatoire.", nameof(presentation));

        Presentation = presentation.Trim();
        _competences.Clear();
        _competences.AddRange(
            competences.Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => c.Trim()));
    }
}
