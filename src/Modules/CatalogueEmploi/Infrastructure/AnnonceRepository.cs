using CVTech.Modules.CatalogueEmploi.Domaine;
using CVTech.Modules.CatalogueEmploi.Infrastructure.Persistence;
using CVTech.Modules.CatalogueEmploi.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace CVTech.Modules.CatalogueEmploi.Infrastructure;

/// <summary>Implémentation EF Core / Azure SQL du port <see cref="IDepotAnnonces"/> (ADR 0005).</summary>
public sealed class AnnonceRepository(EmploiDbContext contexte) : IDepotAnnonces
{
    public async Task AjouterAsync(AnnonceEmploi annonce, CancellationToken ct = default)
    {
        contexte.Annonces.Add(AnnonceEmploiMapper.ToEntity(annonce));
        await contexte.SaveChangesAsync(ct);
    }

    public async Task<AnnonceEmploi?> ObtenirAsync(Guid id, CancellationToken ct = default)
    {
        var entite = await contexte.Annonces.FirstOrDefaultAsync(a => a.Id == id, ct);
        return entite is null ? null : AnnonceEmploiMapper.ToDomain(entite);
    }

    public async Task<IReadOnlyList<AnnonceEmploi>> ListerAsync(
        string? domaineCode = null, CancellationToken ct = default)
    {
        var requete = contexte.Annonces.AsQueryable();
        if (!string.IsNullOrWhiteSpace(domaineCode))
            requete = requete.Where(e => e.DomaineCode == domaineCode);

        var entites = await requete.OrderByDescending(e => e.DatePublication).ToListAsync(ct);
        return entites.Select(AnnonceEmploiMapper.ToDomain).ToList();
    }
}
