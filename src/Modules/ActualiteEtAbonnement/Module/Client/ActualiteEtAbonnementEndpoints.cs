using CVTech.Modules.ActualiteEtAbonnement.Application.Features.ListerNotifications;
using CVTech.Modules.ActualiteEtAbonnement.Application.Features.ObtenirFluxRss;
using CVTech.Modules.ActualiteEtAbonnement.Application.Features.PublierArticle;
using CVTech.Modules.ActualiteEtAbonnement.Application.Features.SAbonner;
using CVTech.Modules.ActualiteEtAbonnement.Domaine;
using CVTech.SharedKernel.Permissions;
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

        // Publication d'un article (Administrateur uniquement).
        actualite.MapPost("/articles", async (PublierArticleRequete r, ISender sender) =>
            await Executer(async () =>
            {
                var id = await sender.Send(new PublierArticleCommand(
                    r.AuteurId, r.Titre, r.Contenu, r.Categorie, r.DomaineLibelle, r.SourceNom, r.SourceUrl));
                return Results.Created($"/actualite/articles/{id}", new { id });
            }));

        // --- D.2 : Abonnement à des domaines (utilisateur authentifié) ---
        actualite.MapPost("/abonnements", async (SAbonnerRequete r, ISender sender) =>
            await Executer(async () =>
            {
                await sender.Send(new SAbonnerCommand(r.UtilisateurId, r.Domaines, r.Canal));
                return Results.NoContent();
            }));

        // Consultation des notifications in-app d'un utilisateur.
        actualite.MapGet("/notifications/{utilisateurId:guid}", async (Guid utilisateurId, ISender sender) =>
            Results.Ok(await sender.Send(new ListerNotificationsQuery(utilisateurId))));

        // Hub SignalR des notifications temps réel.
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
    Guid AuteurId, string Titre, string Contenu, CategorieEditoriale Categorie,
    string? DomaineLibelle, string? SourceNom, string? SourceUrl);
public sealed record SAbonnerRequete(
    Guid UtilisateurId, IReadOnlyList<string> Domaines, CanalDiffusion Canal = CanalDiffusion.InApp);
