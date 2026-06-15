using CVTech.Modules.ActualiteEtAbonnement.Domaine;
using CVTech.Modules.ActualiteEtAbonnement.Infrastructure;
using CVTech.SharedKernel.Domaine;
using FluentAssertions;

namespace CVTech.Modules.ActualiteEtAbonnement.Tests;

public class GenerateurRssTests
{
    private readonly GenerateurRss _generateur = new();

    [Fact]
    public void LeFluxEstUnRss2PointZéroValideContenantLArticle()
    {
        var article = ArticleActualite.Publier(
            Guid.NewGuid(), "Les tendances du recrutement IT en 2026", "Contenu éditorial.",
            CategorieEditoriale.TendancesRecrutement, DomaineMetier.Creer("Cloud Azure"));

        var rss = _generateur.Generer([article], null);

        rss.Should().Contain("<rss version=\"2.0\"");
        rss.Should().Contain("<channel>");
        rss.Should().Contain("<item>");
        rss.Should().Contain("Les tendances du recrutement IT en 2026");
    }

    [Fact]
    public void UnFluxSansArticleNeContientAucunItem()
    {
        var rss = _generateur.Generer([], null);

        rss.Should().Contain("<rss version=\"2.0\"");
        rss.Should().NotContain("<item>");
    }
}
