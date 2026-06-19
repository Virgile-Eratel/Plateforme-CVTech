using CVTech.Modules.ActualiteEtAbonnement.Domaine;
using CVTech.Modules.ActualiteEtAbonnement.Infrastructure.Persistence.Entities;
using CVTech.SharedKernel.Domaine;

namespace CVTech.Modules.ActualiteEtAbonnement.Infrastructure.Persistence.Mappers;

/// <summary>Conversions entre l'agrégat <see cref="ArticleActualite"/> et <see cref="ArticleActualiteEntity"/>.</summary>
public static class ArticleActualiteMapper
{
    public static ArticleActualiteEntity ToEntity(ArticleActualite domaine) => new()
    {
        Id = domaine.Id,
        AuteurId = domaine.AuteurId,
        Titre = domaine.Titre,
        Contenu = domaine.Contenu,
        Categorie = domaine.Categorie,
        DatePublication = domaine.DatePublication,
        DomaineCode = domaine.Domaine?.Code,
        DomaineLibelle = domaine.Domaine?.Libelle,
        SourceNom = domaine.Source?.Nom,
        SourceUrl = domaine.Source?.Url
    };

    public static ArticleActualite ToDomain(ArticleActualiteEntity entite)
    {
        var domaine = entite.DomaineCode is null
            ? null
            : DomaineMetier.Reconstituer(entite.DomaineCode, entite.DomaineLibelle ?? string.Empty);
        var source = entite.SourceNom is null
            ? null
            : SourceExterne.Reconstituer(entite.SourceNom, entite.SourceUrl ?? string.Empty);

        return ArticleActualite.Reconstituer(
            entite.Id, entite.AuteurId, entite.Titre, entite.Contenu,
            entite.Categorie, entite.DatePublication, domaine, source);
    }

    /// <summary>Reporte l'état mutable de l'agrégat sur une entité déjà suivie (mise à jour).</summary>
    public static void Appliquer(ArticleActualite domaine, ArticleActualiteEntity entite)
    {
        entite.AuteurId = domaine.AuteurId;
        entite.Titre = domaine.Titre;
        entite.Contenu = domaine.Contenu;
        entite.Categorie = domaine.Categorie;
        entite.DatePublication = domaine.DatePublication;
        entite.DomaineCode = domaine.Domaine?.Code;
        entite.DomaineLibelle = domaine.Domaine?.Libelle;
        entite.SourceNom = domaine.Source?.Nom;
        entite.SourceUrl = domaine.Source?.Url;
    }
}
