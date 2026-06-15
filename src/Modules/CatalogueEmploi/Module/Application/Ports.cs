using CVTech.Modules.CatalogueEmploi.Domaine;

namespace CVTech.Modules.CatalogueEmploi.Application;

public interface IDepotAnnonces
{
    Task AjouterAsync(AnnonceEmploi annonce, CancellationToken ct = default);
    Task<AnnonceEmploi?> ObtenirAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<AnnonceEmploi>> ListerAsync(string? domaineCode = null, CancellationToken ct = default);
    Task EnregistrerAsync(CancellationToken ct = default);
}

public interface IDepotCv
{
    Task AjouterAsync(CurriculumVitae cv, CancellationToken ct = default);
    Task EnregistrerAsync(CancellationToken ct = default);
}

public interface IDepotCandidatures
{
    Task AjouterAsync(Candidature candidature, CancellationToken ct = default);
    Task EnregistrerAsync(CancellationToken ct = default);
}
