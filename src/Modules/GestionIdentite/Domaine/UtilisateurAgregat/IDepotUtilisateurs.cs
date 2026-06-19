namespace CVTech.Modules.GestionIdentite.Domaine;

/// <summary>Port de persistance des utilisateurs (implémenté dans Infrastructure).</summary>
public interface IDepotUtilisateurs
{
    Task AjouterAsync(Utilisateur utilisateur, CancellationToken ct = default);
    Task<Utilisateur?> ObtenirAsync(Guid id, CancellationToken ct = default);
    Task<Utilisateur?> ObtenirParEmailAsync(string email, CancellationToken ct = default);
    Task MettreAJourAsync(Utilisateur utilisateur, CancellationToken ct = default);
}
