using CVTech.Modules.CatalogueEmploi.Domaine;
using CVTech.Modules.CatalogueEmploi.Infrastructure.Persistence;
using CVTech.Modules.CatalogueEmploi.Infrastructure.Persistence.Mappers;

namespace CVTech.Modules.CatalogueEmploi.Infrastructure;

/// <summary>Implémentation EF Core / Azure SQL du port <see cref="IDepotCandidatures"/> (ADR 0005).</summary>
public sealed class CandidatureRepository(EmploiDbContext contexte) : IDepotCandidatures
{
    public async Task AjouterAsync(Candidature candidature, CancellationToken ct = default)
    {
        contexte.Candidatures.Add(CandidatureMapper.ToEntity(candidature));
        await contexte.SaveChangesAsync(ct);
    }
}
