using System.Security.Claims;
using CVTech.Modules.CatalogueEmploi.Client;
using CVTech.SharedKernel.Securite;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CVTech.Modules.CatalogueEmploi.Application.Features.ConstituerCv;

/// <summary>Constitution / mise à jour d'un CV (Candidat / Admin). Un seul CV par candidat.</summary>
public sealed class ConstituerCvEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder routes) =>
        routes.MapPost("/cv",
            async (ConstituerCvRequete r, ClaimsPrincipal u, ISender sender) =>
            {
                var id = await sender.Send(
                    new ConstituerCvCommand(u.IdUtilisateur(), r.Presentation, r.Competences, r.Age));
                return Results.Created($"/emploi/cv/{id}", new { id });
            }).RequireAuthorization();
}

public sealed record ConstituerCvRequete(string Presentation, IReadOnlyList<string> Competences, int? Age = null);
