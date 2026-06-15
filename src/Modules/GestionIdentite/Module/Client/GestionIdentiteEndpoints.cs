using CVTech.Modules.GestionIdentite.Application.Features.BloquerCompte;
using CVTech.Modules.GestionIdentite.Application.Features.InscrireUtilisateur;
using CVTech.Modules.GestionIdentite.Domaine;
using CVTech.SharedKernel.Permissions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CVTech.Modules.GestionIdentite.Client;

/// <summary>
/// Couche Client : seule porte d'entrée visible du module. Mappe les DTOs vers des
/// Commands MediatR et traduit les exceptions métier en réponses HTTP.
/// </summary>
public static class GestionIdentiteEndpoints
{
    public static IEndpointRouteBuilder MapGestionIdentite(this IEndpointRouteBuilder routes)
    {
        var groupe = routes.MapGroup("/identite").WithTags("GestionIdentite");

        groupe.MapPost("/inscription", async (InscriptionRequete requete, ISender sender) =>
        {
            var id = await sender.Send(new InscrireUtilisateurCommand(requete.Email, requete.Role));
            return Results.Created($"/identite/comptes/{id}", new { id });
        });

        groupe.MapPost("/comptes/{cibleId:guid}/blocage",
            async (Guid cibleId, BlocageRequete requete, ISender sender) =>
            {
                try
                {
                    await sender.Send(new BloquerCompteCommand(requete.AppelantId, cibleId));
                    return Results.NoContent();
                }
                catch (PermissionRefuseeException ex)
                {
                    return Results.Problem(ex.Message, statusCode: StatusCodes.Status403Forbidden);
                }
            });

        return routes;
    }
}

public sealed record InscriptionRequete(string Email, RoleUtilisateur Role);
public sealed record BlocageRequete(Guid AppelantId);
