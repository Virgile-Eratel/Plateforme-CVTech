using System.Security.Claims;
using CVTech.Modules.AppelOffreFreelance.Client;
using CVTech.SharedKernel.Securite;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CVTech.Modules.AppelOffreFreelance.Application.Features.SelectionnerLaureat;

/// <summary>Sélection du lauréat d'un appel d'offre (Entreprise propriétaire / Admin).</summary>
public sealed class SelectionnerLaureatEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder routes) =>
        routes.MapPost("/appels-offre/{appelOffreId:guid}/laureat",
            async (Guid appelOffreId, SelectionnerLaureatRequete r, ClaimsPrincipal u, ISender sender) =>
                await EndpointHttp.Executer(async () =>
                {
                    await sender.Send(new SelectionnerLaureatCommand(u.IdUtilisateur(), appelOffreId, r.PropositionId));
                    return Results.NoContent();
                })).RequireAuthorization();
}

public sealed record SelectionnerLaureatRequete(Guid PropositionId);
