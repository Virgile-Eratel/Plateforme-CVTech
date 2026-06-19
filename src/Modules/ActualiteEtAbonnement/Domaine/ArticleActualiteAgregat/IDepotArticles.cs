namespace CVTech.Modules.ActualiteEtAbonnement.Domaine;

/// <summary>Port de persistance de l'agrégat ArticleActualite (implémenté dans Infrastructure).</summary>
public interface IDepotArticles
{
    Task AjouterAsync(ArticleActualite article, CancellationToken ct = default);
    Task<IReadOnlyList<ArticleActualite>> ListerAsync(string? domaineCode = null, CancellationToken ct = default);
    Task MettreAJourAsync(ArticleActualite article, CancellationToken ct = default);
}
