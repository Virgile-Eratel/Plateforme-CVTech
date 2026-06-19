using CVTech.Modules.CatalogueEmploi.Domaine;
using CVTech.Modules.CatalogueEmploi.Infrastructure.Persistence;
using CVTech.Modules.CatalogueEmploi.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace CVTech.Modules.CatalogueEmploi.Infrastructure;

/// <summary>Implémentation EF Core / Azure SQL du port <see cref="IDepotCv"/> (ADR 0005).</summary>
public sealed class CvRepository(EmploiDbContext contexte) : IDepotCv
{
    public async Task AjouterAsync(CurriculumVitae cv, CancellationToken ct = default)
    {
        contexte.Cvs.Add(CurriculumVitaeMapper.ToEntity(cv));
        await contexte.SaveChangesAsync(ct);
    }

    public async Task<CurriculumVitae?> ObtenirParCandidatAsync(Guid candidatId, CancellationToken ct = default)
    {
        var entite = await contexte.Cvs.FirstOrDefaultAsync(c => c.CandidatId == candidatId, ct);
        return entite is null ? null : CurriculumVitaeMapper.ToDomain(entite);
    }

    public async Task MettreAJourAsync(CurriculumVitae cv, CancellationToken ct = default)
    {
        var entite = await contexte.Cvs.FirstOrDefaultAsync(c => c.Id == cv.Id, ct);
        if (entite is null) return;

        CurriculumVitaeMapper.Appliquer(cv, entite);
        await contexte.SaveChangesAsync(ct);
    }
}
