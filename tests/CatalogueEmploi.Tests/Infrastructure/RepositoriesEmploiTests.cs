using CVTech.Modules.CatalogueEmploi.Domaine;
using CVTech.Modules.CatalogueEmploi.Infrastructure;
using CVTech.Modules.CatalogueEmploi.Infrastructure.Persistence;
using CVTech.SharedKernel.Domaine;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CVTech.Modules.CatalogueEmploi.Tests.Infrastructure;

/// <summary>Persistance EF Core (SQLite) : VO DomaineMetier aplati + collection de compétences.</summary>
public class RepositoriesEmploiTests : IDisposable
{
    private readonly SqliteConnection _connexion;

    public RepositoriesEmploiTests()
    {
        _connexion = new SqliteConnection("DataSource=:memory:");
        _connexion.Open();
        using var ctx = CreerContexte();
        ctx.Database.EnsureCreated();
    }

    private EmploiDbContext CreerContexte() =>
        new(new DbContextOptionsBuilder<EmploiDbContext>().UseSqlite(_connexion).Options);

    [Fact]
    public async Task UneAnnonceEtSonDomaineMetierSontPersistes()
    {
        var domaine = DomaineMetier.Creer("Cloud Azure");
        var annonce = AnnonceEmploi.Publier(
            Guid.NewGuid(), "Ingénieur Cloud", "Mission Azure", TypeContrat.CDI, domaine);
        await using (var ctx = CreerContexte())
        {
            await new AnnonceRepository(ctx).AjouterAsync(annonce);
        }

        await using var lecture = CreerContexte();
        var relue = await new AnnonceRepository(lecture).ObtenirAsync(annonce.Id);

        relue.Should().NotBeNull();
        relue!.Titre.Should().Be("Ingénieur Cloud");
        relue.TypeContrat.Should().Be(TypeContrat.CDI);
        relue.Domaine.Code.Should().Be("cloud-azure");
        relue.Domaine.Libelle.Should().Be("Cloud Azure");
    }

    [Fact]
    public async Task LeFiltreParDomaineNeRetourneQueLesAnnoncesConcernees()
    {
        await using (var ctx = CreerContexte())
        {
            var depot = new AnnonceRepository(ctx);
            await depot.AjouterAsync(AnnonceEmploi.Publier(
                Guid.NewGuid(), "A1", "", TypeContrat.CDI, DomaineMetier.Creer("Cloud Azure")));
            await depot.AjouterAsync(AnnonceEmploi.Publier(
                Guid.NewGuid(), "A2", "", TypeContrat.CDD, DomaineMetier.Creer("Data Science")));
        }

        await using var lecture = CreerContexte();
        var resultat = await new AnnonceRepository(lecture).ListerAsync("cloud-azure");

        resultat.Should().ContainSingle().Which.Titre.Should().Be("A1");
    }

    [Fact]
    public async Task UnCvConserveSesCompetences()
    {
        var cv = CurriculumVitae.Constituer(
            Guid.NewGuid(), "Développeuse senior", new[] { "C#", "Azure", "EF Core" });
        await using (var ctx = CreerContexte())
        {
            await new CvRepository(ctx).AjouterAsync(cv);
        }

        await using var lecture = CreerContexte();
        var relu = await new CvRepository(lecture).ObtenirParCandidatAsync(cv.CandidatId);

        relu.Should().NotBeNull();
        relu!.Presentation.Should().Be("Développeuse senior");
        relu.Competences.Should().BeEquivalentTo("C#", "Azure", "EF Core");
    }

    [Fact]
    public async Task ObtenirParCandidatRetrouveLeCvDuCandidat()
    {
        var candidatId = Guid.NewGuid();
        var cv = CurriculumVitae.Constituer(candidatId, "Présentation", new[] { "C#" });
        await using (var ctx = CreerContexte())
        {
            await new CvRepository(ctx).AjouterAsync(cv);
        }

        await using var lecture = CreerContexte();
        var relu = await new CvRepository(lecture).ObtenirParCandidatAsync(candidatId);

        relu.Should().NotBeNull();
        relu!.CandidatId.Should().Be(candidatId);
        relu.Presentation.Should().Be("Présentation");
    }

    [Fact]
    public async Task ObtenirParCandidatRenvoieNullSansCv()
    {
        await using var lecture = CreerContexte();

        var relu = await new CvRepository(lecture).ObtenirParCandidatAsync(Guid.NewGuid());

        relu.Should().BeNull();
    }

    [Fact]
    public async Task MettreAJourUnCvExistantReviseSaPresentationEtSesCompetences()
    {
        var candidatId = Guid.NewGuid();
        var cv = CurriculumVitae.Constituer(candidatId, "Ancienne", new[] { "C#" });
        await using (var ctx = CreerContexte())
        {
            await new CvRepository(ctx).AjouterAsync(cv);
        }

        cv.MettreAJour("Nouvelle", new[] { "Azure", "Terraform" });
        await using (var ctx = CreerContexte())
        {
            await new CvRepository(ctx).MettreAJourAsync(cv);
        }

        await using var lecture = CreerContexte();
        var relu = await new CvRepository(lecture).ObtenirParCandidatAsync(candidatId);

        relu!.Presentation.Should().Be("Nouvelle");
        relu.Competences.Should().BeEquivalentTo("Azure", "Terraform");
    }

    public void Dispose() => _connexion.Dispose();
}
