using CVTech.Modules.CatalogueEmploi.Application;
using CVTech.Modules.CatalogueEmploi.Application.Features.ConstituerCv;
using CVTech.Modules.CatalogueEmploi.Domaine;
using CVTech.Modules.GestionIdentite.Contracts;
using CVTech.SharedKernel.Permissions;
using FluentAssertions;
using NSubstitute;

namespace CVTech.Modules.CatalogueEmploi.Tests;

public class ConstituerCvHandlerTests
{
    private readonly IVerificateurPermission _permissions = Substitute.For<IVerificateurPermission>();
    private readonly IDepotCv _depot = Substitute.For<IDepotCv>();

    [Fact]
    public async Task UnCandidatBloqueNePeutPasConstituerSonCv()
    {
        var candidatId = Guid.NewGuid();
        _permissions
            .ExigerAsync(candidatId, ActionMetier.ConstituerCv, Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new PermissionRefuseeException()));
        var handler = new ConstituerCvHandler(_permissions, _depot);

        Func<Task> action = () => handler.Handle(
            new ConstituerCvCommand(candidatId, "Présentation", new[] { "C#" }), CancellationToken.None);

        await action.Should().ThrowAsync<PermissionRefuseeException>();
        await _depot.DidNotReceive().AjouterAsync(Arg.Any<CurriculumVitae>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SansCvExistantUnNouveauCvEstCree()
    {
        var candidatId = Guid.NewGuid();
        _depot.ObtenirParCandidatAsync(candidatId, Arg.Any<CancellationToken>())
              .Returns((CurriculumVitae?)null);
        var handler = new ConstituerCvHandler(_permissions, _depot);

        var id = await handler.Handle(
            new ConstituerCvCommand(candidatId, "Développeuse senior", new[] { "C#", "Azure" }),
            CancellationToken.None);

        id.Should().NotBe(Guid.Empty);
        await _depot.Received(1).AjouterAsync(
            Arg.Is<CurriculumVitae>(c => c.CandidatId == candidatId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AvecUnCvExistantCeluiCiEstMisAJourSansEnRecreerUnAutre()
    {
        var candidatId = Guid.NewGuid();
        var existant = CurriculumVitae.Constituer(candidatId, "Ancienne présentation", new[] { "C#" });
        _depot.ObtenirParCandidatAsync(candidatId, Arg.Any<CancellationToken>()).Returns(existant);
        var handler = new ConstituerCvHandler(_permissions, _depot);

        var id = await handler.Handle(
            new ConstituerCvCommand(candidatId, "Présentation à jour", new[] { "Azure", "Terraform" }),
            CancellationToken.None);

        id.Should().Be(existant.Id);
        existant.Presentation.Should().Be("Présentation à jour");
        existant.Competences.Should().BeEquivalentTo("Azure", "Terraform");
        await _depot.DidNotReceive().AjouterAsync(Arg.Any<CurriculumVitae>(), Arg.Any<CancellationToken>());
        await _depot.Received(1).MettreAJourAsync(
            Arg.Is<CurriculumVitae>(c => c.Id == existant.Id), Arg.Any<CancellationToken>());
    }
}
