namespace CVTech.Modules.CatalogueEmploi.Domaine;

/// <summary>Port de persistance de l'agrégat AnnonceEmploi (implémenté dans Infrastructure).</summary>
public interface IDepotAnnonces
{
    Task AjouterAsync(AnnonceEmploi annonce, CancellationToken ct = default);
    Task<AnnonceEmploi?> ObtenirAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<AnnonceEmploi>> ListerAsync(string? domaineCode = null, CancellationToken ct = default);
}
