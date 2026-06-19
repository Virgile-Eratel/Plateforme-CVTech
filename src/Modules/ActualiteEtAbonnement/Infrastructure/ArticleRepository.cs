using CVTech.Modules.ActualiteEtAbonnement.Domaine;
using CVTech.Modules.ActualiteEtAbonnement.Infrastructure.Persistence;
using CVTech.Modules.ActualiteEtAbonnement.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace CVTech.Modules.ActualiteEtAbonnement.Infrastructure;

/// <summary>
/// Implémentation EF Core / Azure SQL du port <see cref="IDepotArticles"/>.
/// Mappe l'agrégat vers/depuis <c>ArticleActualiteEntity</c> et persiste directement (ADR 0005).
/// </summary>
public sealed class ArticleRepository(ActualiteDbContext contexte) : IDepotArticles
{
    public async Task AjouterAsync(ArticleActualite article, CancellationToken ct = default)
    {
        contexte.Articles.Add(ArticleActualiteMapper.ToEntity(article));
        await contexte.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<ArticleActualite>> ListerAsync(
        string? domaineCode = null, CancellationToken ct = default)
    {
        var requete = contexte.Articles.AsQueryable();
        if (!string.IsNullOrWhiteSpace(domaineCode))
            requete = requete.Where(e => e.DomaineCode == domaineCode);

        var entites = await requete.OrderByDescending(e => e.DatePublication).ToListAsync(ct);
        return entites.Select(ArticleActualiteMapper.ToDomain).ToList();
    }

    public async Task MettreAJourAsync(ArticleActualite article, CancellationToken ct = default)
    {
        var entite = await contexte.Articles.FirstOrDefaultAsync(e => e.Id == article.Id, ct);
        if (entite is null) return;

        ArticleActualiteMapper.Appliquer(article, entite);
        await contexte.SaveChangesAsync(ct);
    }
}
