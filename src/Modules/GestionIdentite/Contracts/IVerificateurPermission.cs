namespace CVTech.Modules.GestionIdentite.Contracts;

/// <summary>
/// Contrat public exposé par GestionIdentite (ADR 0004). Seul point d'autorisation
/// de la plateforme : chaque handler de cas d'usage l'interroge AVANT toute action
/// métier. Aucun autre module ne lit la base de GestionIdentite directement.
/// </summary>
public interface IVerificateurPermission
{
    /// <summary>Vrai si l'utilisateur est autorisé pour cette action (sans lever d'exception).</summary>
    Task<bool> EstAutoriseAsync(
        Guid utilisateurId, ActionMetier action, Guid? ressource = null, CancellationToken ct = default);

    /// <summary>
    /// Exige l'autorisation : ne fait rien si autorisé, lève
    /// <c>PermissionRefuseeException</c> sinon. À appeler en première ligne d'un handler.
    /// </summary>
    Task ExigerAsync(
        Guid utilisateurId, ActionMetier action, Guid? ressource = null, CancellationToken ct = default);
}
