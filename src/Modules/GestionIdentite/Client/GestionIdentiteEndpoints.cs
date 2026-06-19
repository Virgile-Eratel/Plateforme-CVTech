using System.Security.Claims;
using CVTech.Modules.GestionIdentite.Application.Features.Authentifier;
using CVTech.Modules.GestionIdentite.Application.Features.BloquerCompte;
using CVTech.Modules.GestionIdentite.Application.Features.InscrireUtilisateur;
using CVTech.Modules.GestionIdentite.Domaine;
using CVTech.SharedKernel.Permissions;
using CVTech.SharedKernel.Securite;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CVTech.Modules.GestionIdentite.Client;

/// <summary>
/// Couche Client : seule porte d'entrée visible du module. Mappe les DTOs vers des
/// Commands MediatR, délivre les jetons JWT et traduit les exceptions métier en réponses HTTP.
/// </summary>
public static class GestionIdentiteEndpoints
{
    public static IEndpointRouteBuilder MapGestionIdentite(this IEndpointRouteBuilder routes)
    {
        var groupe = routes.MapGroup("/identite").WithTags("GestionIdentite");

        // Inscription : action publique. Renvoie immédiatement un jeton (auto-connexion).
        // Sécurité : impossible de s'auto-attribuer le rôle Administrateur → 403.
        groupe.MapPost("/inscription", async (
            InscriptionRequete requete, ISender sender, IGenerateurJeton jetons) =>
        {
            try
            {
                var id = await sender.Send(
                    new InscrireUtilisateurCommand(requete.Email, requete.MotDePasse, requete.Role));
                var jeton = jetons.Generer(id, requete.Email, requete.Role.ToString());
                return Results.Created($"/identite/comptes/{id}",
                    new SessionReponse(id, requete.Email, requete.Role.ToString(), jeton));
            }
            catch (RoleInscriptionInterditException ex)
            {
                return Results.Problem(ex.Message, statusCode: StatusCodes.Status403Forbidden);
            }
        });

        // Connexion : action publique. 401 si identifiants invalides ou compte bloqué.
        groupe.MapPost("/connexion", async (
            ConnexionRequete requete, ISender sender, IGenerateurJeton jetons) =>
        {
            try
            {
                var r = await sender.Send(new AuthentifierCommand(requete.Email, requete.MotDePasse));
                var jeton = jetons.Generer(r.Id, r.Email, r.Role.ToString());
                return Results.Ok(new SessionReponse(r.Id, r.Email, r.Role.ToString(), jeton));
            }
            catch (AuthentificationException ex)
            {
                return Results.Problem(ex.Message, statusCode: StatusCodes.Status401Unauthorized);
            }
        });

        // Blocage : authentifié. L'appelant provient du jeton, jamais du corps de la requête.
        groupe.MapPost("/comptes/{cibleId:guid}/blocage",
            async (Guid cibleId, ClaimsPrincipal utilisateur, ISender sender) =>
            {
                try
                {
                    await sender.Send(new BloquerCompteCommand(utilisateur.IdUtilisateur(), cibleId));
                    return Results.NoContent();
                }
                catch (PermissionRefuseeException ex)
                {
                    return Results.Problem(ex.Message, statusCode: StatusCodes.Status403Forbidden);
                }
            }).RequireAuthorization();

        return routes;
    }
}

public sealed record InscriptionRequete(string Email, string MotDePasse, RoleUtilisateur Role);
public sealed record ConnexionRequete(string Email, string MotDePasse);
public sealed record SessionReponse(Guid Id, string Email, string Role, string Jeton);
