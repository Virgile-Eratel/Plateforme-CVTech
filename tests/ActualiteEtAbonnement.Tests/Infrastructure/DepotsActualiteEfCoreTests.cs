using CVTech.Modules.ActualiteEtAbonnement.Domaine;
using CVTech.Modules.ActualiteEtAbonnement.Infrastructure;
using CVTech.Modules.ActualiteEtAbonnement.Infrastructure.Persistence;
using CVTech.SharedKernel.Domaine;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CVTech.Modules.ActualiteEtAbonnement.Tests.Infrastructure;

/// <summary>
/// Persistance EF Core (SQLite) : owned types nullables (DomaineMetier, SourceExterne)
/// et collection possédée _domaines de l'agrégat Abonnement.
/// </summary>
public class DepotsActualiteEfCoreTests : IDisposable
{
    private readonly SqliteConnection _connexion;

    public DepotsActualiteEfCoreTests()
    {
        _connexion = new SqliteConnection("DataSource=:memory:");
        _connexion.Open();
        using var ctx = CreerContexte();
        ctx.Database.EnsureCreated();
    }

    private ActualiteDbContext CreerContexte() =>
        new(new DbContextOptionsBuilder<ActualiteDbContext>().UseSqlite(_connexion).Options);

    [Fact]
    public async Task UnArticleAvecSonDomaineEtSaSourceEstPersisté()
    {
        var article = ArticleActualite.Publier(
            Guid.NewGuid(), "Tendances 2026", "Contenu éditorial", CategorieEditoriale.Frameworks,
            DomaineMetier.Creer("Cloud Azure"), SourceExterne.Creer("InfoQ", "https://infoq.com"));
        await using (var ctx = CreerContexte())
        {
            await new DepotArticlesEfCore(ctx).AjouterAsync(article);
            await new DepotArticlesEfCore(ctx).EnregistrerAsync();
        }

        await using var lecture = CreerContexte();
        var relus = await new DepotArticlesEfCore(lecture).ListerAsync("cloud-azure");

        relus.Should().ContainSingle();
        var relu = relus[0];
        relu.Titre.Should().Be("Tendances 2026");
        relu.Domaine!.Code.Should().Be("cloud-azure");
        relu.Source!.Nom.Should().Be("InfoQ");
    }

    [Fact]
    public async Task UnArticleSansDomaineNiSourceEstPersisté()
    {
        var article = ArticleActualite.Publier(
            Guid.NewGuid(), "Édito libre", "Sans domaine", CategorieEditoriale.RetourExperience);
        await using (var ctx = CreerContexte())
        {
            await new DepotArticlesEfCore(ctx).AjouterAsync(article);
            await new DepotArticlesEfCore(ctx).EnregistrerAsync();
        }

        await using var lecture = CreerContexte();
        var relus = await new DepotArticlesEfCore(lecture).ListerAsync();

        relus.Should().ContainSingle();
        relus[0].Domaine.Should().BeNull();
        relus[0].Source.Should().BeNull();
    }

    [Fact]
    public async Task UnAbonnementConserveSesDomainesEtPermetDeRetrouverLesAbonnés()
    {
        var utilisateur = Guid.NewGuid();
        var abonnement = Abonnement.Creer(
            utilisateur, [DomaineMetier.Creer("Cloud Azure"), DomaineMetier.Creer("Data Science")]);
        await using (var ctx = CreerContexte())
        {
            await new DepotAbonnementsEfCore(ctx).AjouterOuMettreAJourAsync(abonnement);
            await new DepotAbonnementsEfCore(ctx).EnregistrerAsync();
        }

        await using var lecture = CreerContexte();
        var depot = new DepotAbonnementsEfCore(lecture);
        var relu = await depot.ObtenirParUtilisateurAsync(utilisateur);
        var abonnesCloud = await depot.ListerAbonnesAuDomaineAsync("cloud-azure");

        relu.Should().NotBeNull();
        relu!.Domaines.Should().HaveCount(2);
        relu.EstAbonneAu("data-science").Should().BeTrue();
        abonnesCloud.Should().ContainSingle().Which.Should().Be(utilisateur);
    }

    [Fact]
    public async Task UneNotificationEstPersistéePourSonDestinataire()
    {
        var destinataire = Guid.NewGuid();
        var notification = Notification.Creer(destinataire, "Nouvelle annonce", "Cloud Azure", CanalDiffusion.InApp);
        await using (var ctx = CreerContexte())
        {
            await new DepotNotificationsEfCore(ctx).AjouterAsync(notification);
            await new DepotNotificationsEfCore(ctx).EnregistrerAsync();
        }

        await using var lecture = CreerContexte();
        var relues = await new DepotNotificationsEfCore(lecture).ListerParDestinataireAsync(destinataire);

        relues.Should().ContainSingle().Which.Titre.Should().Be("Nouvelle annonce");
    }

    public void Dispose() => _connexion.Dispose();
}
