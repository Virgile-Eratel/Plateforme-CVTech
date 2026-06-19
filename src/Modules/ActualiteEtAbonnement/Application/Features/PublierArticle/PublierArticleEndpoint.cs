using System.Security.Claims;
using CVTech.Modules.ActualiteEtAbonnement.Client;
using CVTech.Modules.ActualiteEtAbonnement.Domaine;
using CVTech.SharedKernel.Securite;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CVTech.Modules.ActualiteEtAbonnement.Application.Features.PublierArticle;

/// <summary>Publication d'un article (Administrateur uniquement). Auteur issu du jeton.</summary>
public sealed class PublierArticleEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder routes) =>
        routes.MapPost("/articles", async (PublierArticleRequete r, ClaimsPrincipal u, ISender sender) =>
        {
            var id = await sender.Send(new PublierArticleCommand(
                u.IdUtilisateur(), r.Titre, r.Contenu, r.Categorie, r.DomaineLibelle, r.SourceNom, r.SourceUrl));
            return Results.Created($"/actualite/articles/{id}", new { id });
        }).RequireAuthorization();
}

public sealed record PublierArticleRequete(
    string Titre, string Contenu, CategorieEditoriale Categorie,
    string? DomaineLibelle, string? SourceNom, string? SourceUrl);
