using CVTech.Modules.CatalogueEmploi.Domaine;

namespace CVTech.Modules.CatalogueEmploi.Infrastructure.Persistence.Entities;

/// <summary>
/// Entité de persistance EF Core (DAO plat) de l'agrégat <see cref="AnnonceEmploi"/>.
/// Le VO DomaineMetier est aplati en deux colonnes (Code + Libellé).
/// </summary>
public sealed class AnnonceEmploiEntity
{
    public Guid Id { get; set; }
    public Guid EntrepriseId { get; set; }
    public string Titre { get; set; } = default!;
    public string Description { get; set; } = default!;
    public TypeContrat TypeContrat { get; set; }
    public string DomaineCode { get; set; } = default!;
    public string DomaineLibelle { get; set; } = default!;
    public DateTimeOffset DatePublication { get; set; }
}
