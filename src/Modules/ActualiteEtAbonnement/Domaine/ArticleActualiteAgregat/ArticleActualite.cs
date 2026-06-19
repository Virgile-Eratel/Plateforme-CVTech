using CVTech.SharedKernel.Domaine;

namespace CVTech.Modules.ActualiteEtAbonnement.Domaine;

/// <summary>
/// Agrégat : article éditorial du fil d'actualité. Diffusé via RSS. N'est JAMAIS
/// une annonce d'emploi ni un appel d'offre (sous-domaine D.1, strictement éditorial).
/// </summary>
public sealed class ArticleActualite : RacineAgregat<Guid>
{
    public Guid AuteurId { get; private set; }
    public string Titre { get; private set; } = default!;
    public string Contenu { get; private set; } = default!;
    public CategorieEditoriale Categorie { get; private set; }
    public DomaineMetier? Domaine { get; private set; }
    public SourceExterne? Source { get; private set; }
    public DateTimeOffset DatePublication { get; private set; }

    private ArticleActualite() { }

    public static ArticleActualite Publier(
        Guid auteurId,
        string titre,
        string contenu,
        CategorieEditoriale categorie,
        DomaineMetier? domaine = null,
        SourceExterne? source = null)
    {
        if (string.IsNullOrWhiteSpace(titre))
            throw new ArgumentException("Le titre de l'article est obligatoire.", nameof(titre));
        if (string.IsNullOrWhiteSpace(contenu))
            throw new ArgumentException("Le contenu de l'article est obligatoire.", nameof(contenu));

        return new ArticleActualite
        {
            Id = Guid.NewGuid(),
            AuteurId = auteurId,
            Titre = titre.Trim(),
            Contenu = contenu.Trim(),
            Categorie = categorie,
            Domaine = domaine,
            Source = source,
            DatePublication = DateTimeOffset.UtcNow
        };
    }
}
