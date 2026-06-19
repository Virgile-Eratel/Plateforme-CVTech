using CVTech.Modules.AppelOffreFreelance.Domaine;
using CVTech.Modules.AppelOffreFreelance.Infrastructure;
using CVTech.Modules.AppelOffreFreelance.Infrastructure.Persistence;
using CVTech.SharedKernel.Domaine;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CVTech.Modules.AppelOffreFreelance.Tests.Infrastructure;

/// <summary>Persistance EF Core (SQLite) : value objects aplatis CahierDesCharges, BaremeTJM, DomaineMetier.</summary>
public class RepositoriesFreelanceTests : IDisposable
{
    private readonly SqliteConnection _connexion;

    public RepositoriesFreelanceTests()
    {
        _connexion = new SqliteConnection("DataSource=:memory:");
        _connexion.Open();
        using var ctx = CreerContexte();
        ctx.Database.EnsureCreated();
    }

    private FreelanceDbContext CreerContexte() =>
        new(new DbContextOptionsBuilder<FreelanceDbContext>().UseSqlite(_connexion).Options);

    private static CahierDesCharges CahierExemple() => CahierDesCharges.Creer(
        "Refonte du portail", "Site livré en production",
        new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero), 10_000m, 25_000m);

    [Fact]
    public async Task UnAppelOffreEtSonCahierDesChargesSontPersistes()
    {
        var appel = AppelOffre.Publier(
            Guid.NewGuid(), "Mission cloud", CahierExemple(), DomaineMetier.Creer("Cloud Azure"));
        await using (var ctx = CreerContexte())
        {
            await new AppelOffreRepository(ctx).AjouterAsync(appel);
        }

        await using var lecture = CreerContexte();
        var relu = await new AppelOffreRepository(lecture).ObtenirAsync(appel.Id);

        relu.Should().NotBeNull();
        relu!.Titre.Should().Be("Mission cloud");
        relu.Statut.Should().Be(StatutAppelOffre.Ouvert);
        relu.Domaine.Code.Should().Be("cloud-azure");
        relu.CahierDesCharges.BudgetMin.Should().Be(10_000m);
        relu.CahierDesCharges.BudgetMax.Should().Be(25_000m);
        relu.CahierDesCharges.Livrables.Should().Be("Site livré en production");
    }

    [Fact]
    public async Task LaSelectionDUnLaureatEstPersistee()
    {
        var appel = AppelOffre.Publier(
            Guid.NewGuid(), "Mission data", CahierExemple(), DomaineMetier.Creer("Data Science"));
        var laureat = Guid.NewGuid();
        await using (var ctx = CreerContexte())
        {
            var depot = new AppelOffreRepository(ctx);
            await depot.AjouterAsync(appel);
            var charge = await depot.ObtenirAsync(appel.Id);
            charge!.SelectionnerLaureat(laureat);
            await depot.MettreAJourAsync(charge);
        }

        await using var lecture = CreerContexte();
        var relu = await new AppelOffreRepository(lecture).ObtenirAsync(appel.Id);

        relu!.Statut.Should().Be(StatutAppelOffre.Attribue);
        relu.PropositionLaureateId.Should().Be(laureat);
    }

    [Fact]
    public async Task UnePropositionConserveSonTjm()
    {
        var proposition = PropositionFreelance.Soumettre(
            Guid.NewGuid(), Guid.NewGuid(), BaremeTJM.Creer(650m), 20, "Approche agile");
        await using (var ctx = CreerContexte())
        {
            await new PropositionRepository(ctx).AjouterAsync(proposition);
        }

        await using var lecture = CreerContexte();
        var relu = await new PropositionRepository(lecture).ObtenirAsync(proposition.Id);

        relu.Should().NotBeNull();
        relu!.Tjm.MontantJournalier.Should().Be(650m);
        relu.DureeJours.Should().Be(20);
    }

    public void Dispose() => _connexion.Dispose();
}
