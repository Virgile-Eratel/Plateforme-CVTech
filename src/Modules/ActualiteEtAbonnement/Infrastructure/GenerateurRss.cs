using System.Globalization;
using System.Xml.Linq;
using CVTech.Modules.ActualiteEtAbonnement.Application;
using CVTech.Modules.ActualiteEtAbonnement.Domaine;

namespace CVTech.Modules.ActualiteEtAbonnement.Infrastructure;

/// <summary>
/// Génère un flux RSS 2.0 valide à partir des seuls articles éditoriaux.
/// Construit avec System.Xml.Linq (aucune dépendance externe).
/// </summary>
public sealed class GenerateurRss : IGenerateurRss
{
    private const string LienBase = "https://plateforme-cvtech.example/feed/rss";

    public string Generer(IReadOnlyList<ArticleActualite> articles, string? domaineCode)
    {
        var titreCanal = string.IsNullOrWhiteSpace(domaineCode)
            ? "Plateforme-CVTech — Fil d'actualité"
            : $"Plateforme-CVTech — Actualité {domaineCode}";

        var lienCanal = string.IsNullOrWhiteSpace(domaineCode)
            ? LienBase
            : $"{LienBase}?domaine={domaineCode}";

        var items = articles.Select(article => new XElement("item",
            new XElement("title", article.Titre),
            new XElement("description", article.Contenu),
            new XElement("category", article.Categorie.ToString()),
            new XElement("pubDate", article.DatePublication.ToUniversalTime().ToString("r", CultureInfo.InvariantCulture)),
            new XElement("guid", new XAttribute("isPermaLink", "false"), article.Id.ToString())));

        var rss = new XElement("rss",
            new XAttribute("version", "2.0"),
            new XElement("channel",
                new XElement("title", titreCanal),
                new XElement("link", lienCanal),
                new XElement("description", "Articles d'actualité tech publiés par la rédaction de Plateforme-CVTech."),
                new XElement("language", "fr-FR"),
                items));

        var document = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), rss);
        return document.Declaration + Environment.NewLine + document;
    }
}
