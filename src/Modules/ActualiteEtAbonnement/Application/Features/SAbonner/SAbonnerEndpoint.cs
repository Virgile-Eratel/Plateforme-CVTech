using System.Security.Claims;
using CVTech.Modules.ActualiteEtAbonnement.Client;
using CVTech.Modules.ActualiteEtAbonnement.Domaine;
using CVTech.SharedKernel.Securite;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CVTech.Modules.ActualiteEtAbonnement.Application.Features.SAbonner;

/// <summary>Abonnement à des domaines (utilisateur authentifié). Identité issue du jeton.</summary>
public sealed class SAbonnerEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder routes) =>
        routes.MapPost("/abonnements", async (SAbonnerRequete r, ClaimsPrincipal u, ISender sender) =>
        {
            await sender.Send(new SAbonnerCommand(u.IdUtilisateur(), r.Domaines, r.Canal));
            return Results.NoContent();
        }).RequireAuthorization();
}

public sealed record SAbonnerRequete(
    IReadOnlyList<string> Domaines, CanalDiffusion Canal = CanalDiffusion.InApp);
