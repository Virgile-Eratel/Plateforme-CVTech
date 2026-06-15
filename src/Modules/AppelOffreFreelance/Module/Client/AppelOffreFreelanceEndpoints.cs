using CVTech.Modules.AppelOffreFreelance.Application.Features.ListerAppelsOffre;
using CVTech.Modules.AppelOffreFreelance.Application.Features.PublierAppelOffre;
using CVTech.Modules.AppelOffreFreelance.Application.Features.SelectionnerLaureat;
using CVTech.Modules.AppelOffreFreelance.Application.Features.SoumettreProposition;
using CVTech.SharedKernel.Permissions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CVTech.Modules.AppelOffreFreelance.Client;

public static class AppelOffreFreelanceEndpoints
{
    public static IEndpointRouteBuilder MapAppelOffreFreelance(this IEndpointRouteBuilder routes)
    {
        var groupe = routes.MapGroup("/freelance").WithTags("AppelOffreFreelance");

        // Consultation publique (anonyme).
        groupe.MapGet("/appels-offre", async (string? domaine, ISender sender) =>
            Results.Ok(await sender.Send(new ListerAppelsOffreQuery(domaine))));

        // Publication d'un appel d'offre (Entreprise / Admin).
        groupe.MapPost("/appels-offre", async (PublierAppelOffreRequete r, ISender sender) =>
            await Executer(async () =>
            {
                var id = await sender.Send(new PublierAppelOffreCommand(
                    r.EntrepriseId, r.Titre, r.Contexte, r.Livrables, r.Deadline,
                    r.BudgetMin, r.BudgetMax, r.DomaineLibelle));
                return Results.Created($"/freelance/appels-offre/{id}", new { id });
            }));

        // Soumission d'une proposition (Candidat).
        groupe.MapPost("/appels-offre/{appelOffreId:guid}/propositions",
            async (Guid appelOffreId, SoumettrePropositionRequete r, ISender sender) =>
                await Executer(async () =>
                {
                    var id = await sender.Send(new SoumettrePropositionCommand(
                        r.CandidatId, appelOffreId, r.MontantTJM, r.DureeJours, r.Methodologie));
                    return Results.Created($"/freelance/propositions/{id}", new { id });
                }));

        // Sélection du lauréat (Entreprise propriétaire / Admin).
        groupe.MapPost("/appels-offre/{appelOffreId:guid}/laureat",
            async (Guid appelOffreId, SelectionnerLaureatRequete r, ISender sender) =>
                await Executer(async () =>
                {
                    await sender.Send(new SelectionnerLaureatCommand(r.AppelantId, appelOffreId, r.PropositionId));
                    return Results.NoContent();
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

public sealed record PublierAppelOffreRequete(
    Guid EntrepriseId, string Titre, string Contexte, string Livrables,
    DateTimeOffset Deadline, decimal BudgetMin, decimal BudgetMax, string DomaineLibelle);
public sealed record SoumettrePropositionRequete(
    Guid CandidatId, decimal MontantTJM, int DureeJours, string Methodologie);
public sealed record SelectionnerLaureatRequete(Guid AppelantId, Guid PropositionId);
