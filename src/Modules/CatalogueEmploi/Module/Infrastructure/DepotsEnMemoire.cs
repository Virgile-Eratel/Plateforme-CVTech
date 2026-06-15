using System.Collections.Concurrent;
using CVTech.Modules.CatalogueEmploi.Application;
using CVTech.Modules.CatalogueEmploi.Domaine;

namespace CVTech.Modules.CatalogueEmploi.Infrastructure;

// Dépôts en mémoire (pilote). Remplacés par EF Core / Azure SQL en Phase 3 bis.

public sealed class DepotAnnoncesEnMemoire : IDepotAnnonces
{
    private readonly ConcurrentDictionary<Guid, AnnonceEmploi> _annonces = new();

    public Task AjouterAsync(AnnonceEmploi annonce, CancellationToken ct = default)
    {
        _annonces[annonce.Id] = annonce;
        return Task.CompletedTask;
    }

    public Task<AnnonceEmploi?> ObtenirAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(_annonces.GetValueOrDefault(id));

    public Task<IReadOnlyList<AnnonceEmploi>> ListerAsync(string? domaineCode = null, CancellationToken ct = default)
    {
        IEnumerable<AnnonceEmploi> resultat = _annonces.Values.OrderByDescending(a => a.DatePublication);
        if (!string.IsNullOrWhiteSpace(domaineCode))
            resultat = resultat.Where(a => a.Domaine.Code == domaineCode);
        return Task.FromResult<IReadOnlyList<AnnonceEmploi>>(resultat.ToList());
    }

    public Task EnregistrerAsync(CancellationToken ct = default) => Task.CompletedTask;
}

public sealed class DepotCvEnMemoire : IDepotCv
{
    private readonly ConcurrentDictionary<Guid, CurriculumVitae> _cvs = new();

    public Task AjouterAsync(CurriculumVitae cv, CancellationToken ct = default)
    {
        _cvs[cv.Id] = cv;
        return Task.CompletedTask;
    }

    public Task EnregistrerAsync(CancellationToken ct = default) => Task.CompletedTask;
}

public sealed class DepotCandidaturesEnMemoire : IDepotCandidatures
{
    private readonly ConcurrentDictionary<Guid, Candidature> _candidatures = new();

    public Task AjouterAsync(Candidature candidature, CancellationToken ct = default)
    {
        _candidatures[candidature.Id] = candidature;
        return Task.CompletedTask;
    }

    public Task EnregistrerAsync(CancellationToken ct = default) => Task.CompletedTask;
}
