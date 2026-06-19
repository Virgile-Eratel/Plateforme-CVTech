using CVTech.Modules.CatalogueEmploi.Client;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CVTech.Modules.CatalogueEmploi.Application.Features.ListerAnnonces;

/// <summary>Consultation publique des annonces : action anonyme, sans permission.</summary>
public sealed class ListerAnnoncesEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder routes) =>
        routes.MapGet("/annonces", async (string? domaine, ISender sender) =>
            Results.Ok(await sender.Send(new ListerAnnoncesQuery(domaine))));
}
