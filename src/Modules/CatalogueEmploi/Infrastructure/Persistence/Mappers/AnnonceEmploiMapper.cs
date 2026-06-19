using CVTech.Modules.CatalogueEmploi.Domaine;
using CVTech.Modules.CatalogueEmploi.Infrastructure.Persistence.Entities;
using CVTech.SharedKernel.Domaine;

namespace CVTech.Modules.CatalogueEmploi.Infrastructure.Persistence.Mappers;

/// <summary>Conversions entre l'agrégat <see cref="AnnonceEmploi"/> et l'entité EF <see cref="AnnonceEmploiEntity"/>.</summary>
public static class AnnonceEmploiMapper
{
    public static AnnonceEmploiEntity ToEntity(AnnonceEmploi domaine) => new()
    {
        Id = domaine.Id,
        EntrepriseId = domaine.EntrepriseId,
        Titre = domaine.Titre,
        Description = domaine.Description,
        TypeContrat = domaine.TypeContrat,
        DomaineCode = domaine.Domaine.Code,
        DomaineLibelle = domaine.Domaine.Libelle,
        DatePublication = domaine.DatePublication
    };

    public static AnnonceEmploi ToDomain(AnnonceEmploiEntity entite) =>
        AnnonceEmploi.Reconstituer(
            entite.Id, entite.EntrepriseId, entite.Titre, entite.Description, entite.TypeContrat,
            DomaineMetier.Reconstituer(entite.DomaineCode, entite.DomaineLibelle), entite.DatePublication);

    /// <summary>Reporte l'état mutable de l'agrégat sur une entité déjà suivie (mise à jour).</summary>
    public static void Appliquer(AnnonceEmploi domaine, AnnonceEmploiEntity entite)
    {
        entite.EntrepriseId = domaine.EntrepriseId;
        entite.Titre = domaine.Titre;
        entite.Description = domaine.Description;
        entite.TypeContrat = domaine.TypeContrat;
        entite.DomaineCode = domaine.Domaine.Code;
        entite.DomaineLibelle = domaine.Domaine.Libelle;
        entite.DatePublication = domaine.DatePublication;
    }
}
