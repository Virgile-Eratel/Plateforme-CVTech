using System.Security.Claims;
using CVTech.Modules.CatalogueEmploi.Client;
using CVTech.SharedKernel.Securite;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CVTech.Modules.CatalogueEmploi.Application.Features.ConsulterMonCv;

/// <summary>Consultation de SON propre CV (Candidat / Admin). 200 avec le CV, ou 204 si aucun CV.</summary>
public sealed class ConsulterMonCvEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder routes) =>
        routes.MapGet("/mon-cv",
            async (ClaimsPrincipal u, ISender sender) =>
            {
                var cv = await sender.Send(new ConsulterMonCvQuery(u.IdUtilisateur()));
                return cv is null ? Results.NoContent() : Results.Ok(cv);
            }).RequireAuthorization();
}
