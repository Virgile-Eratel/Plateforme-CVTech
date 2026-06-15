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
        if (string.IsNullOrWhiteSpace(presentation))
            throw new ArgumentException("La présentation du CV est obligatoire.", nameof(presentation));

        var cv = new CurriculumVitae
        {
            Id = Guid.NewGuid(),
            CandidatId = candidatId,
            Presentation = presentation.Trim()
        };
        cv._competences.AddRange(
            competences.Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => c.Trim()));
        return cv;
    }
}
