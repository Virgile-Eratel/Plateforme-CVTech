using CVTech.Modules.AppelOffreFreelance.Client;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CVTech.Modules.AppelOffreFreelance.Application.Features.ListerAppelsOffre;

/// <summary>Consultation publique des appels d'offre : action anonyme, sans permission.</summary>
public sealed class ListerAppelsOffreEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder routes) =>
        routes.MapGet("/appels-offre", async (string? domaine, ISender sender) =>
            Results.Ok(await sender.Send(new ListerAppelsOffreQuery(domaine))));
}
