using CVTech.Modules.AppelOffreFreelance.Domaine;
using CVTech.Modules.AppelOffreFreelance.Infrastructure.Persistence;
using CVTech.Modules.AppelOffreFreelance.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace CVTech.Modules.AppelOffreFreelance.Infrastructure;

/// <summary>
/// Implémentation EF Core / Azure SQL du port <see cref="IDepotAppelsOffre"/> (ADR 0005).
/// Mappe l'agrégat vers/depuis <c>AppelOffreEntity</c> et persiste directement.
/// </summary>
public sealed class AppelOffreRepository(FreelanceDbContext contexte) : IDepotAppelsOffre
{
    public async Task AjouterAsync(AppelOffre appelOffre, CancellationToken ct = default)
    {
        await contexte.AppelsOffre.AddAsync(AppelOffreMapper.ToEntity(appelOffre), ct);
        await contexte.SaveChangesAsync(ct);
    }

    public async Task<AppelOffre?> ObtenirAsync(Guid id, CancellationToken ct = default)
    {
        var entite = await contexte.AppelsOffre.FirstOrDefaultAsync(a => a.Id == id, ct);
        return entite is null ? null : AppelOffreMapper.ToDomain(entite);
    }

    public async Task<IReadOnlyList<AppelOffre>> ListerAsync(
        string? domaineCode = null, CancellationToken ct = default)
    {
        var requete = contexte.AppelsOffre.AsQueryable();
        if (!string.IsNullOrWhiteSpace(domaineCode))
            requete = requete.Where(e => e.DomaineCode == domaineCode);

        var entites = await requete.OrderByDescending(e => e.DatePublication).ToListAsync(ct);
        return entites.Select(AppelOffreMapper.ToDomain).ToList();
    }

    public async Task MettreAJourAsync(AppelOffre appelOffre, CancellationToken ct = default)
    {
        var entite = await contexte.AppelsOffre.FirstOrDefaultAsync(a => a.Id == appelOffre.Id, ct);
        if (entite is null) return;

        AppelOffreMapper.Appliquer(appelOffre, entite);
        await contexte.SaveChangesAsync(ct);
    }
}
