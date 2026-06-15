using CVTech.Modules.CatalogueEmploi.Application;
using CVTech.Modules.CatalogueEmploi.Domaine;
using CVTech.Modules.CatalogueEmploi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CVTech.Modules.CatalogueEmploi.Infrastructure;

// Implémentations EF Core / Azure SQL des ports de persistance (ADR 0005).
// Les contrats consommés par l'Application restent inchangés.

public sealed class DepotAnnoncesEfCore(EmploiDbContext contexte) : IDepotAnnonces
{
    public async Task AjouterAsync(AnnonceEmploi annonce, CancellationToken ct = default) =>
        await contexte.Annonces.AddAsync(annonce, ct);

    public Task<AnnonceEmploi?> ObtenirAsync(Guid id, CancellationToken ct = default) =>
        contexte.Annonces.FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<IReadOnlyList<AnnonceEmploi>> ListerAsync(
        string? domaineCode = null, CancellationToken ct = default)
    {
        var requete = contexte.Annonces.AsQueryable();
        if (!string.IsNullOrWhiteSpace(domaineCode))
            requete = requete.Where(a => a.Domaine.Code == domaineCode);
        return await requete.OrderByDescending(a => a.DatePublication).ToListAsync(ct);
    }

    public Task EnregistrerAsync(CancellationToken ct = default) => contexte.SaveChangesAsync(ct);
}

public sealed class DepotCvEfCore(EmploiDbContext contexte) : IDepotCv
{
    public async Task AjouterAsync(CurriculumVitae cv, CancellationToken ct = default) =>
        await contexte.Cvs.AddAsync(cv, ct);

    public Task EnregistrerAsync(CancellationToken ct = default) => contexte.SaveChangesAsync(ct);
}

public sealed class DepotCandidaturesEfCore(EmploiDbContext contexte) : IDepotCandidatures
{
    public async Task AjouterAsync(Candidature candidature, CancellationToken ct = default) =>
        await contexte.Candidatures.AddAsync(candidature, ct);

    public Task EnregistrerAsync(CancellationToken ct = default) => contexte.SaveChangesAsync(ct);
}
