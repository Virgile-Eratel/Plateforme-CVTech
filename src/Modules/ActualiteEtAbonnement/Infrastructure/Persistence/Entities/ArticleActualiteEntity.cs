using CVTech.Modules.ActualiteEtAbonnement.Domaine;

namespace CVTech.Modules.ActualiteEtAbonnement.Infrastructure.Persistence.Entities;

/// <summary>
/// Entité de persistance EF Core (DAO plat) de l'agrégat <see cref="ArticleActualite"/>.
/// Aucune logique métier : les VO optionnels Domaine et Source sont aplatis en colonnes nullables.
/// </summary>
public sealed class ArticleActualiteEntity
{
    public Guid Id { get; set; }
    public Guid AuteurId { get; set; }
    public string Titre { get; set; } = default!;
    public string Contenu { get; set; } = default!;
    public CategorieEditoriale Categorie { get; set; }
    public DateTimeOffset DatePublication { get; set; }

    // DomaineMetier optionnel (un article éditorial peut n'en cibler aucun).
    public string? DomaineCode { get; set; }
    public string? DomaineLibelle { get; set; }

    // SourceExterne optionnelle.
    public string? SourceNom { get; set; }
    public string? SourceUrl { get; set; }
}
