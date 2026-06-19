using System.Security.Claims;
using CVTech.Modules.CatalogueEmploi.Client;
using CVTech.SharedKernel.Securite;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CVTech.Modules.CatalogueEmploi.Application.Features.PostulerAnnonce;

/// <summary>Candidature à une annonce (Candidat). Identité issue du jeton.</summary>
public sealed class PostulerAnnonceEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder routes) =>
        routes.MapPost("/annonces/{annonceId:guid}/candidatures",
            async (Guid annonceId, PostulerRequete r, ClaimsPrincipal u, ISender sender) =>
            {
                var id = await sender.Send(
                    new PostulerAnnonceCommand(u.IdUtilisateur(), annonceId, r.LettreMotivation));
                return Results.Created($"/emploi/candidatures/{id}", new { id });
            }).RequireAuthorization();
}

public sealed record PostulerRequete(string? LettreMotivation);
