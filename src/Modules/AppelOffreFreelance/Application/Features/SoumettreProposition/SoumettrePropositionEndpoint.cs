using System.Security.Claims;
using CVTech.Modules.AppelOffreFreelance.Client;
using CVTech.SharedKernel.Securite;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CVTech.Modules.AppelOffreFreelance.Application.Features.SoumettreProposition;

/// <summary>Soumission d'une proposition freelance (Candidat). Identité issue du jeton.</summary>
public sealed class SoumettrePropositionEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder routes) =>
        routes.MapPost("/appels-offre/{appelOffreId:guid}/propositions",
            async (Guid appelOffreId, SoumettrePropositionRequete r, ClaimsPrincipal u, ISender sender) =>
                await EndpointHttp.Executer(async () =>
                {
                    var id = await sender.Send(new SoumettrePropositionCommand(
                        u.IdUtilisateur(), appelOffreId, r.MontantTJM, r.DureeJours, r.Methodologie));
                    return Results.Created($"/freelance/propositions/{id}", new { id });
                })).RequireAuthorization();
}

public sealed record SoumettrePropositionRequete(decimal MontantTJM, int DureeJours, string Methodologie);
