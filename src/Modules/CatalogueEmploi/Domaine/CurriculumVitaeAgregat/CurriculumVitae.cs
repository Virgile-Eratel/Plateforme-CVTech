using CVTech.SharedKernel.Domaine;

namespace CVTech.Modules.CatalogueEmploi.Domaine;

/// <summary>Agrégat : CV structuré d'un candidat, réutilisable pour ses candidatures.</summary>
public sealed class CurriculumVitae : RacineAgregat<Guid>
{
    private readonly List<string> _competences = [];

    /// <summary>Âge légal minimal et maximal admis sur un CV (fourchette de vie active plausible).</summary>
    private const int AgeMinimal = 16;
    private const int AgeMaximal = 100;

    public Guid CandidatId { get; private set; }
    public string Presentation { get; private set; } = default!;
    public IReadOnlyCollection<string> Competences => _competences.AsReadOnly();

    /// <summary>Âge déclaré par le candidat. Optionnel (donnée sensible non obligatoire sur un CV).</summary>
    public int? Age { get; private set; }

    private CurriculumVitae() { }

    public static CurriculumVitae Constituer(
        Guid candidatId, string presentation, IEnumerable<string> competences, int? age = null)
    {
        var cv = new CurriculumVitae { Id = Guid.NewGuid(), CandidatId = candidatId };
        cv.Appliquer(presentation, competences, age);
        return cv;
    }

    /// <summary>Réhydrate l'agrégat depuis la persistance (mapper Infrastructure), Id préservé.</summary>
    public static CurriculumVitae Reconstituer(
        Guid id, Guid candidatId, string presentation, IEnumerable<string> competences, int? age = null)
    {
        var cv = new CurriculumVitae
        {
            Id = id, CandidatId = candidatId, Presentation = presentation, Age = age
        };
        cv._competences.AddRange(competences);
        return cv;
    }

    /// <summary>Met à jour le CV existant (un seul CV par candidat, on le révise au lieu d'en recréer).</summary>
    public void MettreAJour(string presentation, IEnumerable<string> competences, int? age = null) =>
        Appliquer(presentation, competences, age);

    private void Appliquer(string presentation, IEnumerable<string> competences, int? age)
    {
        if (string.IsNullOrWhiteSpace(presentation))
            throw new ArgumentException("La présentation du CV est obligatoire.", nameof(presentation));

        if (age is < AgeMinimal or > AgeMaximal)
            throw new ArgumentException(
                $"L'âge indiqué sur le CV doit être compris entre {AgeMinimal} et {AgeMaximal} ans.", nameof(age));

        Presentation = presentation.Trim();
        Age = age;
        _competences.Clear();
        _competences.AddRange(
            competences.Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => c.Trim()));
    }
}
