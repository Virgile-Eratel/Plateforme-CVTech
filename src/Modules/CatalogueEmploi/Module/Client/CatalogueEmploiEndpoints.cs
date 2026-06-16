using System.Security.Claims;
using CVTech.Modules.CatalogueEmploi.Application.Features.ConstituerCv;
using CVTech.Modules.CatalogueEmploi.Application.Features.ConsulterMonCv;
using CVTech.Modules.CatalogueEmploi.Application.Features.ListerAnnonces;
using CVTech.Modules.CatalogueEmploi.Application.Features.PostulerAnnonce;
using CVTech.Modules.CatalogueEmploi.Application.Features.PublierAnnonce;
using CVTech.Modules.CatalogueEmploi.Domaine;
using CVTech.SharedKernel.Permissions;
using CVTech.SharedKernel.Securite;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CVTech.Modules.CatalogueEmploi.Client;

public static class CatalogueEmploiEndpoints
{
    public static IEndpointRouteBuilder MapCatalogueEmploi(this IEndpointRouteBuilder routes)
    {
        var groupe = routes.MapGroup("/emploi").WithTags("CatalogueEmploi");

        // Consultation publique (anonyme).
        groupe.MapGet("/annonces", async (string? domaine, ISender sender) =>
            Results.Ok(await sender.Send(new ListerAnnoncesQuery(domaine))));

        // Publication d'une annonce (Entreprise / Admin). L'identité provient du jeton.
        groupe.MapPost("/annonces", async (PublierAnnonceRequete r, ClaimsPrincipal u, ISender sender) =>
            await Executer(async () =>
            {
                var id = await sender.Send(new PublierAnnonceCommand(
                    u.IdUtilisateur(), r.Titre, r.Description, r.TypeContrat, r.DomaineLibelle));
                return Results.Created($"/emploi/annonces/{id}", new { id });
            })).RequireAuthorization();

        // Constitution / mise à jour d'un CV (Candidat / Admin). Un seul CV par candidat.
        groupe.MapPost("/cv", async (ConstituerCvRequete r, ClaimsPrincipal u, ISender sender) =>
            await Executer(async () =>
            {
                var id = await sender.Send(new ConstituerCvCommand(u.IdUtilisateur(), r.Presentation, r.Competences));
                return Results.Created($"/emploi/cv/{id}", new { id });
            })).RequireAuthorization();

        // Consultation de SON propre CV (Candidat / Admin). 200 avec le CV, ou 204 si aucun CV.
        groupe.MapGet("/mon-cv", async (ClaimsPrincipal u, ISender sender) =>
            await Executer(async () =>
            {
                var cv = await sender.Send(new ConsulterMonCvQuery(u.IdUtilisateur()));
                return cv is null ? Results.NoContent() : Results.Ok(cv);
            })).RequireAuthorization();

        // Candidature à une annonce (Candidat).
        groupe.MapPost("/annonces/{annonceId:guid}/candidatures",
            async (Guid annonceId, PostulerRequete r, ClaimsPrincipal u, ISender sender) =>
                await Executer(async () =>
                {
                    var id = await sender.Send(
                        new PostulerAnnonceCommand(u.IdUtilisateur(), annonceId, r.LettreMotivation));
                    return Results.Created($"/emploi/candidatures/{id}", new { id });
                })).RequireAuthorization();

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

public sealed record PublierAnnonceRequete(
    string Titre, string Description, TypeContrat TypeContrat, string DomaineLibelle);
public sealed record ConstituerCvRequete(string Presentation, IReadOnlyList<string> Competences);
public sealed record PostulerRequete(string? LettreMotivation);
