using CVTech.Modules.GestionIdentite.Application.Features.Authentifier;
using CVTech.Modules.GestionIdentite.Client;
using CVTech.Modules.GestionIdentite.Domaine;
using CVTech.SharedKernel.Securite;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CVTech.Modules.GestionIdentite.Application.Features.InscrireUtilisateur;

/// <summary>
/// Inscription : action publique. Renvoie immédiatement un jeton (auto-connexion).
/// Sécurité : impossible de s'auto-attribuer le rôle Administrateur → 403.
/// </summary>
public sealed class InscrireUtilisateurEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder routes) =>
        routes.MapPost("/inscription", async (
            InscriptionRequete requete, ISender sender, IGenerateurJeton jetons) =>
        {
            // Le rôle effectif vient du résultat de la commande (source autoritative), jamais
            // de la requête : aucun rôle privilégié ne peut être obtenu par auto-inscription.
            var resultat = await sender.Send(
                new InscrireUtilisateurCommand(requete.Email, requete.MotDePasse, requete.Role));
            var role = resultat.Role.ToString();
            var jeton = jetons.Generer(resultat.Id, requete.Email, role);
            return Results.Created($"/identite/comptes/{resultat.Id}",
                new SessionReponse(resultat.Id, requete.Email, role, jeton));
        });
}

public sealed record InscriptionRequete(string Email, string MotDePasse, RoleUtilisateur Role);
