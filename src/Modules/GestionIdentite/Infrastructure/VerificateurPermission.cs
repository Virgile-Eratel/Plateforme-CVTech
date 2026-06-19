using CVTech.Modules.GestionIdentite.Application;
using CVTech.Modules.GestionIdentite.Contracts;
using CVTech.Modules.GestionIdentite.Domaine;
using CVTech.SharedKernel.Permissions;

namespace CVTech.Modules.GestionIdentite.Infrastructure;

/// <summary>
/// Implémentation du contrat public <see cref="IVerificateurPermission"/>.
/// Confronte le rôle de l'utilisateur à <see cref="MatricePermissions"/> et refuse
/// systématiquement tout compte inconnu ou bloqué.
/// </summary>
public sealed class VerificateurPermission(IDepotUtilisateurs depot) : IVerificateurPermission
{
    public async Task<bool> EstAutoriseAsync(
        Guid utilisateurId, ActionMetier action, Guid? ressource = null, CancellationToken ct = default)
    {
        var utilisateur = await depot.ObtenirAsync(utilisateurId, ct);

        // Compte inconnu ou bloqué : aucune action authentifiée n'est permise.
        if (utilisateur is null || utilisateur.EstBloque)
            return false;

        return MatricePermissions.EstAutorise(utilisateur.Role, action);
    }

    public async Task ExigerAsync(
        Guid utilisateurId, ActionMetier action, Guid? ressource = null, CancellationToken ct = default)
    {
        if (!await EstAutoriseAsync(utilisateurId, action, ressource, ct))
            throw new PermissionRefuseeException(
                $"L'utilisateur {utilisateurId} n'est pas autorisé à exécuter l'action {action}.");
    }
}
