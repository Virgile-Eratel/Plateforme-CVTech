using CVTech.Modules.AppelOffreFreelance.Domaine;

namespace CVTech.Modules.AppelOffreFreelance.Application;

public interface IDepotAppelsOffre
{
    Task AjouterAsync(AppelOffre appelOffre, CancellationToken ct = default);
    Task<AppelOffre?> ObtenirAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<AppelOffre>> ListerAsync(string? domaineCode = null, CancellationToken ct = default);
    Task EnregistrerAsync(CancellationToken ct = default);
}

public interface IDepotPropositions
{
    Task AjouterAsync(PropositionFreelance proposition, CancellationToken ct = default);
    Task<PropositionFreelance?> ObtenirAsync(Guid id, CancellationToken ct = default);
    Task EnregistrerAsync(CancellationToken ct = default);
}
