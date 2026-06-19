using System.Security.Claims;
using CVTech.Modules.ActualiteEtAbonnement.Client;
using CVTech.SharedKernel.Securite;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CVTech.Modules.ActualiteEtAbonnement.Application.Features.ListerNotifications;

/// <summary>Consultation de SES propres notifications in-app (identité issue du jeton).</summary>
public sealed class ListerNotificationsEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder routes) =>
        routes.MapGet("/notifications", async (ClaimsPrincipal u, ISender sender) =>
            Results.Ok(await sender.Send(new ListerNotificationsQuery(u.IdUtilisateur()))))
            .RequireAuthorization();
}
