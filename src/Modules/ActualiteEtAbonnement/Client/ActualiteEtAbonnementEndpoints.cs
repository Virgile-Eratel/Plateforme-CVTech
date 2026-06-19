using CVTech.Modules.ActualiteEtAbonnement.Application.Features.ObtenirFluxRss;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CVTech.Modules.ActualiteEtAbonnement.Client;

/// <summary>
/// Seule porte d'entrée du module. Branche le port de sortie et délègue le mapping des
/// endpoints applicatifs (Application/Features/**) au groupe « /actualite ». Deux points
/// d'entrée techniques restent câblés explicitement ici car hors de ce groupe : le flux
/// RSS public anonyme et le transport temps réel SignalR.
/// </summary>
public static class ActualiteEtAbonnementEndpoints
{
    public static IEndpointRouteBuilder MapActualiteEtAbonnement(this IEndpointRouteBuilder routes)
    {
        // Flux RSS éditorial : PUBLIC et ANONYME, hors groupe /actualite (syndication standard).
        // La route et sa délégation MediatR vivent dans la slice (ObtenirFluxRssEndpoint) ;
        // ici on se contente de la brancher sur la racine, sans aucune logique.
        new ObtenirFluxRssEndpoint().Map(routes);

        // Endpoints applicatifs du module, sous le groupe /actualite.
        var actualite = routes.MapGroup("/actualite").WithTags("Actualite");
        actualite.AppliquerPresentateur();
        actualite.BrancherEndpoints();

        // Transport temps réel des notifications in-app (SignalR, cf. NotificationsHub).
        routes.MapHub<NotificationsHub>("/hubs/notifications");

        return routes;
    }
}
