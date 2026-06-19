using CVTech.Modules.ActualiteEtAbonnement.Client;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CVTech.Modules.ActualiteEtAbonnement.Application.Features.ObtenirFluxRss;

/// <summary>
/// Flux RSS éditorial : PUBLIC et ANONYME, hors groupe « /actualite » (syndication standard).
/// L'endpoint ne fait que mapper la route et déléguer à MediatR ; aucune logique métier ici.
/// </summary>
public sealed class ObtenirFluxRssEndpoint : IEndpointAutonome
{
    public void Map(IEndpointRouteBuilder routes) =>
        routes.MapGet("/feed/rss", async (string? domaine, ISender sender) =>
        {
            var rss = await sender.Send(new ObtenirFluxRssQuery(domaine));
            return Results.Content(rss, "application/rss+xml; charset=utf-8");
        }).WithTags("Actualite");
}
