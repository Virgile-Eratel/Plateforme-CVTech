using CVTech.Modules.GestionIdentite.Domaine;
using MediatR;

namespace CVTech.Modules.GestionIdentite.Application.Features.InscrireUtilisateur;

public sealed class InscrireUtilisateurHandler(IDepotUtilisateurs depot, IHacheurMotDePasse hacheur)
    : IRequestHandler<InscrireUtilisateurCommand, InscriptionResultat>
{
    public async Task<InscriptionResultat> Handle(InscrireUtilisateurCommand commande, CancellationToken ct)
    {
        // Inscription = action publique : aucune vérification de permission requise.
        // Le rôle est validé par l'invariant du domaine (Administrateur interdit ici).
        var utilisateur = Utilisateur.Inscrire(commande.Email, commande.Role);
        utilisateur.DefinirMotDePasse(hacheur.Hacher(commande.MotDePasse));

        await depot.AjouterAsync(utilisateur, ct);

        // On renvoie le rôle RÉELLEMENT persisté : le jeton ne doit pas dépendre de la requête.
        return new InscriptionResultat(utilisateur.Id, utilisateur.Role);
    }
}
