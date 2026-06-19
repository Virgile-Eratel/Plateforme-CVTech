namespace CVTech.Modules.CatalogueEmploi.Domaine;

/// <summary>Port de persistance de l'agrégat CurriculumVitae (implémenté dans Infrastructure).</summary>
public interface IDepotCv
{
    Task AjouterAsync(CurriculumVitae cv, CancellationToken ct = default);
    Task<CurriculumVitae?> ObtenirParCandidatAsync(Guid candidatId, CancellationToken ct = default);
    Task MettreAJourAsync(CurriculumVitae cv, CancellationToken ct = default);
}
