using CVTech.Modules.AppelOffreFreelance.Domaine;
using CVTech.Modules.AppelOffreFreelance.Infrastructure.Persistence;
using CVTech.Modules.AppelOffreFreelance.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace CVTech.Modules.AppelOffreFreelance.Infrastructure;

/// <summary>
/// Implémentation EF Core / Azure SQL du port <see cref="IDepotPropositions"/> (ADR 0005).
/// Mappe l'agrégat vers/depuis <c>PropositionFreelanceEntity</c> et persiste directement.
/// </summary>
public sealed class PropositionRepository(FreelanceDbContext contexte) : IDepotPropositions
{
    public async Task AjouterAsync(PropositionFreelance proposition, CancellationToken ct = default)
    {
        await contexte.Propositions.AddAsync(PropositionFreelanceMapper.ToEntity(proposition), ct);
        await contexte.SaveChangesAsync(ct);
    }

    public async Task<PropositionFreelance?> ObtenirAsync(Guid id, CancellationToken ct = default)
    {
        var entite = await contexte.Propositions.FirstOrDefaultAsync(p => p.Id == id, ct);
        return entite is null ? null : PropositionFreelanceMapper.ToDomain(entite);
    }
}
