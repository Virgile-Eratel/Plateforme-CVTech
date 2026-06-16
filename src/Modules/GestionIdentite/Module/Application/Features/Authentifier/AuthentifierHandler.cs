using MediatR;

namespace CVTech.Modules.GestionIdentite.Application.Features.Authentifier;

public sealed class AuthentifierHandler(IDepotUtilisateurs depot, IHacheurMotDePasse hacheur)
    : IRequestHandler<AuthentifierCommand, ResultatAuthentification>
{
    public async Task<ResultatAuthentification> Handle(AuthentifierCommand commande, CancellationToken ct)
    {
        var utilisateur = await depot.ObtenirParEmailAsync(commande.Email.Trim(), ct);

        // Compte inconnu, sans mot de passe défini, ou bloqué : refus (message générique).
        if (utilisateur is null || utilisateur.MotDePasseHash is null || utilisateur.EstBloque)
            throw new AuthentificationException();

        if (!hacheur.Verifier(utilisateur.MotDePasseHash, commande.MotDePasse))
            throw new AuthentificationException();

        return new ResultatAuthentification(utilisateur.Id, utilisateur.Email, utilisateur.Role);
    }
}
