namespace CVTech.Modules.AppelOffreFreelance.Domaine;

/// <summary>Port de persistance de l'agrégat AppelOffre (implémenté dans Infrastructure).</summary>
public interface IDepotAppelsOffre
{
    Task AjouterAsync(AppelOffre appelOffre, CancellationToken ct = default);
    Task<AppelOffre?> ObtenirAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<AppelOffre>> ListerAsync(string? domaineCode = null, CancellationToken ct = default);
    Task MettreAJourAsync(AppelOffre appelOffre, CancellationToken ct = default);
}
