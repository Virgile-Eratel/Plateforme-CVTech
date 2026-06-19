using CVTech.Modules.CatalogueEmploi.Domaine;
using CVTech.Modules.CatalogueEmploi.Infrastructure.Persistence.Entities;

namespace CVTech.Modules.CatalogueEmploi.Infrastructure.Persistence.Mappers;

/// <summary>Conversions entre l'agrégat <see cref="Candidature"/> et l'entité EF <see cref="CandidatureEntity"/>.</summary>
public static class CandidatureMapper
{
    public static CandidatureEntity ToEntity(Candidature domaine) => new()
    {
        Id = domaine.Id,
        AnnonceId = domaine.AnnonceId,
        CandidatId = domaine.CandidatId,
        LettreMotivation = domaine.LettreMotivation,
        DateDepot = domaine.DateDepot
    };

    public static Candidature ToDomain(CandidatureEntity entite) =>
        Candidature.Reconstituer(
            entite.Id, entite.AnnonceId, entite.CandidatId, entite.LettreMotivation, entite.DateDepot);

    /// <summary>Reporte l'état mutable de l'agrégat sur une entité déjà suivie (mise à jour).</summary>
    public static void Appliquer(Candidature domaine, CandidatureEntity entite)
    {
        entite.AnnonceId = domaine.AnnonceId;
        entite.CandidatId = domaine.CandidatId;
        entite.LettreMotivation = domaine.LettreMotivation;
        entite.DateDepot = domaine.DateDepot;
    }
}
