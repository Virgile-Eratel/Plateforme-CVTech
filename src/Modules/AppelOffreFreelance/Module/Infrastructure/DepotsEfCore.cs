using CVTech.Modules.AppelOffreFreelance.Application;
using CVTech.Modules.AppelOffreFreelance.Domaine;
using CVTech.Modules.AppelOffreFreelance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CVTech.Modules.AppelOffreFreelance.Infrastructure;

// Implémentations EF Core / Azure SQL des ports de persistance (ADR 0005).

public sealed class DepotAppelsOffreEfCore(FreelanceDbContext contexte) : IDepotAppelsOffre
{
    public async Task AjouterAsync(AppelOffre appelOffre, CancellationToken ct = default) =>
        await contexte.AppelsOffre.AddAsync(appelOffre, ct);

    public Task<AppelOffre?> ObtenirAsync(Guid id, CancellationToken ct = default) =>
        contexte.AppelsOffre.FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<IReadOnlyList<AppelOffre>> ListerAsync(
        string? domaineCode = null, CancellationToken ct = default)
    {
        var requete = contexte.AppelsOffre.AsQueryable();
        if (!string.IsNullOrWhiteSpace(domaineCode))
            requete = requete.Where(a => a.Domaine.Code == domaineCode);
        return await requete.OrderByDescending(a => a.DatePublication).ToListAsync(ct);
    }

    public Task EnregistrerAsync(CancellationToken ct = default) => contexte.SaveChangesAsync(ct);
}

public sealed class DepotPropositionsEfCore(FreelanceDbContext contexte) : IDepotPropositions
{
    public async Task AjouterAsync(PropositionFreelance proposition, CancellationToken ct = default) =>
        await contexte.Propositions.AddAsync(proposition, ct);

    public Task<PropositionFreelance?> ObtenirAsync(Guid id, CancellationToken ct = default) =>
        contexte.Propositions.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task EnregistrerAsync(CancellationToken ct = default) => contexte.SaveChangesAsync(ct);
}
