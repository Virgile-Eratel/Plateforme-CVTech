using CVTech.Modules.CatalogueEmploi.Domaine;
using CVTech.Modules.CatalogueEmploi.Infrastructure.Persistence.Entities;

namespace CVTech.Modules.CatalogueEmploi.Infrastructure.Persistence.Mappers;

/// <summary>Conversions entre l'agrégat <see cref="CurriculumVitae"/> et l'entité EF <see cref="CurriculumVitaeEntity"/>.</summary>
public static class CurriculumVitaeMapper
{
    public static CurriculumVitaeEntity ToEntity(CurriculumVitae domaine) => new()
    {
        Id = domaine.Id,
        CandidatId = domaine.CandidatId,
        Presentation = domaine.Presentation,
        Competences = domaine.Competences.ToList()
    };

    public static CurriculumVitae ToDomain(CurriculumVitaeEntity entite) =>
        CurriculumVitae.Reconstituer(entite.Id, entite.CandidatId, entite.Presentation, entite.Competences);

    /// <summary>Reporte l'état mutable de l'agrégat sur une entité déjà suivie (mise à jour).</summary>
    public static void Appliquer(CurriculumVitae domaine, CurriculumVitaeEntity entite)
    {
        entite.CandidatId = domaine.CandidatId;
        entite.Presentation = domaine.Presentation;
        entite.Competences = domaine.Competences.ToList();
    }
}
