using CVTech.Modules.CatalogueEmploi.Domaine;
using CVTech.Modules.CatalogueEmploi.Infrastructure;
using CVTech.Modules.CatalogueEmploi.Infrastructure.Persistence;
using CVTech.SharedKernel.Domaine;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CVTech.Modules.CatalogueEmploi.Tests.Infrastructure;

/// <summary>Persistance EF Core (SQLite) : owned type DomaineMetier + collection de compétences.</summary>
public class DepotsEmploiEfCoreTests : IDisposable
{
    private readonly SqliteConnection _connexion;

    public DepotsEmploiEfCoreTests()
    {
        _connexion = new SqliteConnection("DataSource=:memory:");
        _connexion.Open();
        using var ctx = CreerContexte();
        ctx.Database.EnsureCreated();
    }

    private EmploiDbContext CreerContexte() =>
        new(new DbContextOptionsBuilder<EmploiDbContext>().UseSqlite(_connexion).Options);

    [Fact]
    public async Task UneAnnonceEtSonDomaineMetierSontPersistés()
    {
        var domaine = DomaineMetier.Creer("Cloud Azure");
        var annonce = AnnonceEmploi.Publier(
            Guid.NewGuid(), "Ingénieur Cloud", "Mission Azure", TypeContrat.CDI, domaine);
        await using (var ctx = CreerContexte())
        {
            await new DepotAnnoncesEfCore(ctx).AjouterAsync(annonce);
            await new DepotAnnoncesEfCore(ctx).EnregistrerAsync();
        }

        await using var lecture = CreerContexte();
        var relue = await new DepotAnnoncesEfCore(lecture).ObtenirAsync(annonce.Id);

        relue.Should().NotBeNull();
        relue!.Titre.Should().Be("Ingénieur Cloud");
        relue.TypeContrat.Should().Be(TypeContrat.CDI);
        relue.Domaine.Code.Should().Be("cloud-azure");
        relue.Domaine.Libelle.Should().Be("Cloud Azure");
    }

    [Fact]
    public async Task LeFiltreParDomaineNeRetourneQueLesAnnoncesConcernées()
    {
        await using (var ctx = CreerContexte())
        {
            var depot = new DepotAnnoncesEfCore(ctx);
            await depot.AjouterAsync(AnnonceEmploi.Publier(
                Guid.NewGuid(), "A1", "", TypeContrat.CDI, DomaineMetier.Creer("Cloud Azure")));
            await depot.AjouterAsync(AnnonceEmploi.Publier(
                Guid.NewGuid(), "A2", "", TypeContrat.CDD, DomaineMetier.Creer("Data Science")));
            await depot.EnregistrerAsync();
        }

        await using var lecture = CreerContexte();
        var resultat = await new DepotAnnoncesEfCore(lecture).ListerAsync("cloud-azure");

        resultat.Should().ContainSingle().Which.Titre.Should().Be("A1");
    }

    [Fact]
    public async Task UnCvConserveSesCompétences()
    {
        var cv = CurriculumVitae.Constituer(
            Guid.NewGuid(), "Développeuse senior", new[] { "C#", "Azure", "EF Core" });
        await using (var ctx = CreerContexte())
        {
            await new DepotCvEfCore(ctx).AjouterAsync(cv);
            await new DepotCvEfCore(ctx).EnregistrerAsync();
        }

        await using var lecture = CreerContexte();
        var relu = await lecture.Set<CurriculumVitae>().FirstAsync(c => c.Id == cv.Id);

        relu.Presentation.Should().Be("Développeuse senior");
        relu.Competences.Should().BeEquivalentTo("C#", "Azure", "EF Core");
    }

    [Fact]
    public async Task ObtenirParCandidatRetrouveLeCvDuCandidat()
    {
        var candidatId = Guid.NewGuid();
        var cv = CurriculumVitae.Constituer(candidatId, "Présentation", new[] { "C#" });
        await using (var ctx = CreerContexte())
        {
            await new DepotCvEfCore(ctx).AjouterAsync(cv);
            await new DepotCvEfCore(ctx).EnregistrerAsync();
        }

        await using var lecture = CreerContexte();
        var relu = await new DepotCvEfCore(lecture).ObtenirParCandidatAsync(candidatId);

        relu.Should().NotBeNull();
        relu!.CandidatId.Should().Be(candidatId);
        relu.Presentation.Should().Be("Présentation");
    }

    [Fact]
    public async Task ObtenirParCandidatRenvoieNullSansCv()
    {
        await using var lecture = CreerContexte();

        var relu = await new DepotCvEfCore(lecture).ObtenirParCandidatAsync(Guid.NewGuid());

        relu.Should().BeNull();
    }

    public void Dispose() => _connexion.Dispose();
}
