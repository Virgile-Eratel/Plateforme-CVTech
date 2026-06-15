using CVTech.Modules.CatalogueEmploi.Application.Features.ConstituerCv;
using CVTech.Modules.CatalogueEmploi.Application.Features.ListerAnnonces;
using CVTech.Modules.CatalogueEmploi.Application.Features.PostulerAnnonce;
using CVTech.Modules.CatalogueEmploi.Application.Features.PublierAnnonce;
using CVTech.Modules.CatalogueEmploi.Domaine;
using CVTech.SharedKernel.Permissions;
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

        // Publication d'une annonce (Entreprise / Admin).
        groupe.MapPost("/annonces", async (PublierAnnonceRequete r, ISender sender) =>
            await Executer(async () =>
            {
                var id = await sender.Send(new PublierAnnonceCommand(
                    r.EntrepriseId, r.Titre, r.Description, r.TypeContrat, r.DomaineLibelle));
                return Results.Created($"/emploi/annonces/{id}", new { id });
            }));

        // Constitution d'un CV (Candidat / Admin).
        groupe.MapPost("/cv", async (ConstituerCvRequete r, ISender sender) =>
            await Executer(async () =>
            {
                var id = await sender.Send(new ConstituerCvCommand(r.CandidatId, r.Presentation, r.Competences));
                return Results.Created($"/emploi/cv/{id}", new { id });
            }));

        // Candidature à une annonce (Candidat).
        groupe.MapPost("/annonces/{annonceId:guid}/candidatures",
            async (Guid annonceId, PostulerRequete r, ISender sender) =>
                await Executer(async () =>
                {
                    var id = await sender.Send(
                        new PostulerAnnonceCommand(r.CandidatId, annonceId, r.LettreMotivation));
                    return Results.Created($"/emploi/candidatures/{id}", new { id });
                }));

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
    Guid EntrepriseId, string Titre, string Description, TypeContrat TypeContrat, string DomaineLibelle);
public sealed record ConstituerCvRequete(Guid CandidatId, string Presentation, IReadOnlyList<string> Competences);
public sealed record PostulerRequete(Guid CandidatId, string? LettreMotivation);
