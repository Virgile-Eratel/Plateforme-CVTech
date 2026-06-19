using CVTech.Modules.GestionIdentite.Client;
using CVTech.SharedKernel.Securite;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CVTech.Modules.GestionIdentite.Application.Features.Authentifier;

/// <summary>Connexion : action publique. 401 si identifiants invalides ou compte bloqué.</summary>
public sealed class AuthentifierEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder routes) =>
        routes.MapPost("/connexion", async (
            ConnexionRequete requete, ISender sender, IGenerateurJeton jetons) =>
        {
            var r = await sender.Send(new AuthentifierCommand(requete.Email, requete.MotDePasse));
            var jeton = jetons.Generer(r.Id, r.Email, r.Role.ToString());
            return Results.Ok(new SessionReponse(r.Id, r.Email, r.Role.ToString(), jeton));
        });
}

public sealed record ConnexionRequete(string Email, string MotDePasse);
public sealed record SessionReponse(Guid Id, string Email, string Role, string Jeton);
