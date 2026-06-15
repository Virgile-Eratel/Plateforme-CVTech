using System.Collections.Concurrent;
using CVTech.Modules.AppelOffreFreelance.Application;
using CVTech.Modules.AppelOffreFreelance.Domaine;

namespace CVTech.Modules.AppelOffreFreelance.Infrastructure;

public sealed class DepotAppelsOffreEnMemoire : IDepotAppelsOffre
{
    private readonly ConcurrentDictionary<Guid, AppelOffre> _appels = new();

    public Task AjouterAsync(AppelOffre appelOffre, CancellationToken ct = default)
    {
        _appels[appelOffre.Id] = appelOffre;
        return Task.CompletedTask;
    }

    public Task<AppelOffre?> ObtenirAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(_appels.GetValueOrDefault(id));

    public Task<IReadOnlyList<AppelOffre>> ListerAsync(string? domaineCode = null, CancellationToken ct = default)
    {
        IEnumerable<AppelOffre> resultat = _appels.Values.OrderByDescending(a => a.DatePublication);
        if (!string.IsNullOrWhiteSpace(domaineCode))
            resultat = resultat.Where(a => a.Domaine.Code == domaineCode);
        return Task.FromResult<IReadOnlyList<AppelOffre>>(resultat.ToList());
    }

    public Task EnregistrerAsync(CancellationToken ct = default) => Task.CompletedTask;
}

public sealed class DepotPropositionsEnMemoire : IDepotPropositions
{
    private readonly ConcurrentDictionary<Guid, PropositionFreelance> _propositions = new();

    public Task AjouterAsync(PropositionFreelance proposition, CancellationToken ct = default)
    {
        _propositions[proposition.Id] = proposition;
        return Task.CompletedTask;
    }

    public Task<PropositionFreelance?> ObtenirAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(_propositions.GetValueOrDefault(id));

    public Task EnregistrerAsync(CancellationToken ct = default) => Task.CompletedTask;
}
