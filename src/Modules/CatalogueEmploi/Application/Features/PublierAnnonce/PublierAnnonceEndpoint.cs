using System.Security.Claims;
using CVTech.Modules.CatalogueEmploi.Client;
using CVTech.Modules.CatalogueEmploi.Domaine;
using CVTech.SharedKernel.Securite;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CVTech.Modules.CatalogueEmploi.Application.Features.PublierAnnonce;

/// <summary>Publication d'une annonce (Entreprise / Admin). Identité issue du jeton.</summary>
public sealed class PublierAnnonceEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder routes) =>
        routes.MapPost("/annonces",
            async (PublierAnnonceRequete r, ClaimsPrincipal u, ISender sender) =>
            {
                var id = await sender.Send(new PublierAnnonceCommand(
                    u.IdUtilisateur(), r.Titre, r.Description, r.TypeContrat, r.DomaineLibelle));
                return Results.Created($"/emploi/annonces/{id}", new { id });
            }).RequireAuthorization();
}

public sealed record PublierAnnonceRequete(
    string Titre, string Description, TypeContrat TypeContrat, string DomaineLibelle);
