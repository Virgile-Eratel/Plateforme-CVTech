using CVTech.Modules.GestionIdentite.Domaine;

namespace CVTech.Modules.GestionIdentite.Application;

/// <summary>Port de persistance des utilisateurs (implémenté dans Infrastructure).</summary>
public interface IDepotUtilisateurs
{
    Task AjouterAsync(Utilisateur utilisateur, CancellationToken ct = default);
    Task<Utilisateur?> ObtenirAsync(Guid id, CancellationToken ct = default);
    Task EnregistrerAsync(CancellationToken ct = default);
}
