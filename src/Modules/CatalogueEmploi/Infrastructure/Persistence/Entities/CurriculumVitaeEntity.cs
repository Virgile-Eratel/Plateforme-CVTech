using CVTech.Modules.CatalogueEmploi.Domaine;

namespace CVTech.Modules.CatalogueEmploi.Infrastructure.Persistence.Entities;

/// <summary>
/// Entité de persistance EF Core (DAO plat) de l'agrégat <see cref="CurriculumVitae"/>.
/// </summary>
public sealed class CurriculumVitaeEntity
{
    public Guid Id { get; set; }
    public Guid CandidatId { get; set; }
    public string Presentation { get; set; } = default!;
    public int? Age { get; set; }
    public List<string> Competences { get; set; } = [];
}
