using System.Security.Claims;
using CVTech.Modules.GestionIdentite.Client;
using CVTech.SharedKernel.Securite;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CVTech.Modules.GestionIdentite.Application.Features.BloquerCompte;

/// <summary>Blocage : authentifié. L'appelant provient du jeton, jamais du corps de la requête.</summary>
public sealed class BloquerCompteEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder routes) =>
        routes.MapPost("/comptes/{cibleId:guid}/blocage",
            async (Guid cibleId, ClaimsPrincipal utilisateur, ISender sender) =>
            {
                await sender.Send(new BloquerCompteCommand(utilisateur.IdUtilisateur(), cibleId));
                return Results.NoContent();
            }).RequireAuthorization();
}
