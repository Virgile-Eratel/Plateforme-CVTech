using CVTech.Modules.GestionIdentite.Domaine;
using MediatR;

namespace CVTech.Modules.GestionIdentite.Application.Features.InscrireUtilisateur;

public sealed class InscrireUtilisateurHandler(IDepotUtilisateurs depot)
    : IRequestHandler<InscrireUtilisateurCommand, Guid>
{
    public async Task<Guid> Handle(InscrireUtilisateurCommand commande, CancellationToken ct)
    {
        // Inscription = action publique : aucune vérification de permission requise.
        var utilisateur = Utilisateur.Inscrire(commande.Email, commande.Role);

        await depot.AjouterAsync(utilisateur, ct);
        await depot.EnregistrerAsync(ct);

        return utilisateur.Id;
    }
}
