namespace CVTech.Modules.CatalogueEmploi.Domaine;

/// <summary>Port de persistance de l'agrégat Candidature (implémenté dans Infrastructure).</summary>
public interface IDepotCandidatures
{
    Task AjouterAsync(Candidature candidature, CancellationToken ct = default);
}
