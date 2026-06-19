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
/// Persistance EF Core (SQLite) via entités plates + mappers : VO optionnels (DomaineMetier,
/// SourceExterne) aplatis en colonnes nullables, et collection de domaines de l'agrégat Abonnement.
/// </summary>
public class ActualiteRepositoriesTests : IDisposable
{
    private readonly SqliteConnection _connexion;

    public ActualiteRepositoriesTests()
    {
        _connexion = new SqliteConnection("DataSource=:memory:");
        _connexion.Open();
        using var ctx = CreerContexte();
        ctx.Database.EnsureCreated();
    }

    private ActualiteDbContext CreerContexte() =>
        new(new DbContextOptionsBuilder<ActualiteDbContext>().UseSqlite(_connexion).Options);

    [Fact]
    public async Task UnArticleAvecSonDomaineEtSaSourceEstPersiste()
    {
        var article = ArticleActualite.Publier(
            Guid.NewGuid(), "Tendances 2026", "Contenu éditorial", CategorieEditoriale.Frameworks,
            DomaineMetier.Creer("Cloud Azure"), SourceExterne.Creer("InfoQ", "https://infoq.com"));
        await using (var ctx = CreerContexte())
        {
            await new ArticleRepository(ctx).AjouterAsync(article);
        }

        await using var lecture = CreerContexte();
        var relus = await new ArticleRepository(lecture).ListerAsync("cloud-azure");

        relus.Should().ContainSingle();
        var relu = relus[0];
        relu.Titre.Should().Be("Tendances 2026");
        relu.Domaine!.Code.Should().Be("cloud-azure");
        relu.Source!.Nom.Should().Be("InfoQ");
    }

    [Fact]
    public async Task UnArticleSansDomaineNiSourceEstPersiste()
    {
        var article = ArticleActualite.Publier(
            Guid.NewGuid(), "Édito libre", "Sans domaine", CategorieEditoriale.RetourExperience);
        await using (var ctx = CreerContexte())
        {
            await new ArticleRepository(ctx).AjouterAsync(article);
        }

        await using var lecture = CreerContexte();
        var relus = await new ArticleRepository(lecture).ListerAsync();

        relus.Should().ContainSingle();
        relus[0].Domaine.Should().BeNull();
        relus[0].Source.Should().BeNull();
    }

    [Fact]
    public async Task UnAbonnementConserveSesDomainesEtPermetDeRetrouverLesAbonnes()
    {
        var utilisateur = Guid.NewGuid();
        var abonnement = Abonnement.Creer(
            utilisateur, [DomaineMetier.Creer("Cloud Azure"), DomaineMetier.Creer("Data Science")]);
        await using (var ctx = CreerContexte())
        {
            await new AbonnementRepository(ctx).AjouterOuMettreAJourAsync(abonnement);
        }

        await using var lecture = CreerContexte();
        var depot = new AbonnementRepository(lecture);
        var relu = await depot.ObtenirParUtilisateurAsync(utilisateur);
        var abonnesCloud = await depot.ListerAbonnesAuDomaineAsync("cloud-azure");

        relu.Should().NotBeNull();
        relu!.Domaines.Should().HaveCount(2);
        relu.EstAbonneAu("data-science").Should().BeTrue();
        abonnesCloud.Should().ContainSingle().Which.Should().Be(utilisateur);
    }

    [Fact]
    public async Task UnSecondAbonnementDuMemeUtilisateurEstFusionne()
    {
        // Sémantique upsert préservée : un seul abonnement par utilisateur, domaines cumulés.
        var utilisateur = Guid.NewGuid();
        await using (var ctx = CreerContexte())
            await new AbonnementRepository(ctx).AjouterOuMettreAJourAsync(
                Abonnement.Creer(utilisateur, [DomaineMetier.Creer("Cloud Azure")]));

        await using (var ctx = CreerContexte())
        {
            var depot = new AbonnementRepository(ctx);
            var existant = await depot.ObtenirParUtilisateurAsync(utilisateur);
            existant!.AjouterDomaines([DomaineMetier.Creer("Data Science")]);
            await depot.AjouterOuMettreAJourAsync(existant);
        }

        await using var lecture = CreerContexte();
        var relu = await new AbonnementRepository(lecture).ObtenirParUtilisateurAsync(utilisateur);

        relu!.Domaines.Should().HaveCount(2);
    }

    [Fact]
    public async Task UneNotificationEstPersisteePourSonDestinataire()
    {
        var destinataire = Guid.NewGuid();
        var notification = Notification.Creer(destinataire, "Nouvelle annonce", "Cloud Azure", CanalDiffusion.InApp);
        await using (var ctx = CreerContexte())
        {
            await new NotificationRepository(ctx).AjouterAsync(notification);
        }

        await using var lecture = CreerContexte();
        var relues = await new NotificationRepository(lecture).ListerParDestinataireAsync(destinataire);

        relues.Should().ContainSingle().Which.Titre.Should().Be("Nouvelle annonce");
    }

    public void Dispose() => _connexion.Dispose();
}
