using System.Security.Claims;
using CVTech.Modules.ActualiteEtAbonnement.Application.Features.ListerNotifications;
using CVTech.Modules.ActualiteEtAbonnement.Application.Features.ObtenirFluxRss;
using CVTech.Modules.ActualiteEtAbonnement.Application.Features.PublierArticle;
using CVTech.Modules.ActualiteEtAbonnement.Application.Features.SAbonner;
using CVTech.Modules.ActualiteEtAbonnement.Domaine;
using CVTech.SharedKernel.Permissions;
using CVTech.SharedKernel.Securite;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CVTech.Modules.ActualiteEtAbonnement.Client;

public static class ActualiteEtAbonnementEndpoints
{
    public static IEndpointRouteBuilder MapActualiteEtAbonnement(this IEndpointRouteBuilder routes)
    {
        // --- D.1 : Flux RSS éditorial, PUBLIC et ANONYME ---
        routes.MapGet("/feed/rss", async (string? domaine, ISender sender) =>
        {
            var rss = await sender.Send(new ObtenirFluxRssQuery(domaine));
            return Results.Content(rss, "application/rss+xml; charset=utf-8");
        }).WithTags("Actualite");

        var actualite = routes.MapGroup("/actualite").WithTags("Actualite");

        // Publication d'un article (Administrateur uniquement). Auteur issu du jeton.
        actualite.MapPost("/articles", async (PublierArticleRequete r, ClaimsPrincipal u, ISender sender) =>
            await Executer(async () =>
            {
                var id = await sender.Send(new PublierArticleCommand(
                    u.IdUtilisateur(), r.Titre, r.Contenu, r.Categorie, r.DomaineLibelle, r.SourceNom, r.SourceUrl));
                return Results.Created($"/actualite/articles/{id}", new { id });
            })).RequireAuthorization();

        // --- D.2 : Abonnement à des domaines (utilisateur authentifié) ---
        actualite.MapPost("/abonnements", async (SAbonnerRequete r, ClaimsPrincipal u, ISender sender) =>
            await Executer(async () =>
            {
                await sender.Send(new SAbonnerCommand(u.IdUtilisateur(), r.Domaines, r.Canal));
                return Results.NoContent();
            })).RequireAuthorization();

        // Consultation de SES propres notifications in-app (identité issue du jeton).
        actualite.MapGet("/notifications", async (ClaimsPrincipal u, ISender sender) =>
            Results.Ok(await sender.Send(new ListerNotificationsQuery(u.IdUtilisateur()))))
            .RequireAuthorization();

        // Hub SignalR des notifications temps réel (authentifié, cf. NotificationsHub).
        routes.MapHub<NotificationsHub>("/hubs/notifications");

        return routes;
    }

    private static async Task<IResult> Executer(Func<Task<IResult>> action)
    {
        try { return await action(); }
        catch (PermissionRefuseeException ex)
        {
            return Results.Problem(ex.Message, statusCode: StatusCodes.Status403Forbidden);
        }
    }
}

public sealed record PublierArticleRequete(
    string Titre, string Contenu, CategorieEditoriale Categorie,
    string? DomaineLibelle, string? SourceNom, string? SourceUrl);
public sealed record SAbonnerRequete(
    IReadOnlyList<string> Domaines, CanalDiffusion Canal = CanalDiffusion.InApp);
