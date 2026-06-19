namespace CVTech.Modules.ActualiteEtAbonnement.Domaine;

/// <summary>Port de persistance de l'agrégat Abonnement (implémenté dans Infrastructure).</summary>
public interface IDepotAbonnements
{
    Task<Abonnement?> ObtenirParUtilisateurAsync(Guid utilisateurId, CancellationToken ct = default);
    Task AjouterOuMettreAJourAsync(Abonnement abonnement, CancellationToken ct = default);
    Task<IReadOnlyList<Guid>> ListerAbonnesAuDomaineAsync(string domaineCode, CancellationToken ct = default);
}
