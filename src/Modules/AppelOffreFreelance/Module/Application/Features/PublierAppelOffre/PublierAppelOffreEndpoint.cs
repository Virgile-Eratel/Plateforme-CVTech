using System.Security.Claims;
using CVTech.Modules.AppelOffreFreelance.Client;
using CVTech.SharedKernel.Securite;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CVTech.Modules.AppelOffreFreelance.Application.Features.PublierAppelOffre;

/// <summary>Publication d'un appel d'offre (Entreprise / Admin). Identité issue du jeton.</summary>
public sealed class PublierAppelOffreEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder routes) =>
        routes.MapPost("/appels-offre",
            async (PublierAppelOffreRequete r, ClaimsPrincipal u, ISender sender) =>
                await EndpointHttp.Executer(async () =>
                {
                    var id = await sender.Send(new PublierAppelOffreCommand(
                        u.IdUtilisateur(), r.Titre, r.Contexte, r.Livrables, r.Deadline,
                        r.BudgetMin, r.BudgetMax, r.DomaineLibelle));
                    return Results.Created($"/freelance/appels-offre/{id}", new { id });
                })).RequireAuthorization();
}

public sealed record PublierAppelOffreRequete(
    string Titre, string Contexte, string Livrables,
    DateTimeOffset Deadline, decimal BudgetMin, decimal BudgetMax, string DomaineLibelle);
