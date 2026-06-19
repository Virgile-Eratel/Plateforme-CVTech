using MediatR;

using CVTech.Modules.ActualiteEtAbonnement.Domaine;

namespace CVTech.Modules.ActualiteEtAbonnement.Application.Features.ObtenirFluxRss;

/// <summary>
/// Génère le flux RSS 2.0 public (anonyme). Contient EXCLUSIVEMENT les articles
/// éditoriaux ; jamais d'annonces ni d'appels d'offre. Filtre optionnel par domaine.
/// </summary>
public sealed record ObtenirFluxRssQuery(string? DomaineCode = null) : IRequest<string>;

public sealed class ObtenirFluxRssHandler(IDepotArticles depot, IGenerateurRss generateur)
    : IRequestHandler<ObtenirFluxRssQuery, string>
{
    public async Task<string> Handle(ObtenirFluxRssQuery requete, CancellationToken ct)
    {
        var articles = await depot.ListerAsync(requete.DomaineCode, ct);
        return generateur.Generer(articles, requete.DomaineCode);
    }
}
