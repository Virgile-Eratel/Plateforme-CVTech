using CVTech.Modules.CatalogueEmploi.Domaine;

namespace CVTech.Modules.CatalogueEmploi.Infrastructure.Persistence.Entities;

/// <summary>
/// Entité de persistance EF Core (DAO plat) de l'agrégat <see cref="Candidature"/>.
/// </summary>
public sealed class CandidatureEntity
{
    public Guid Id { get; set; }
    public Guid AnnonceId { get; set; }
    public Guid CandidatId { get; set; }
    public string? LettreMotivation { get; set; }
    public DateTimeOffset DateDepot { get; set; }
}
