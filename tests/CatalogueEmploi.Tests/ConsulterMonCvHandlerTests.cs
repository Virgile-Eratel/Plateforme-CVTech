using CVTech.Modules.CatalogueEmploi.Application;
using CVTech.Modules.CatalogueEmploi.Application.Features.ConsulterMonCv;
using CVTech.Modules.CatalogueEmploi.Domaine;
using CVTech.Modules.GestionIdentite.Contracts;
using CVTech.SharedKernel.Permissions;
using FluentAssertions;
using NSubstitute;

namespace CVTech.Modules.CatalogueEmploi.Tests;

public class ConsulterMonCvHandlerTests
{
    private readonly IVerificateurPermission _permissions = Substitute.For<IVerificateurPermission>();
    private readonly IDepotCv _depot = Substitute.For<IDepotCv>();

    [Fact]
    public async Task UnCompteBloqueNePeutPasConsulterSonCv()
    {
        var candidatId = Guid.NewGuid();
        _permissions
            .ExigerAsync(candidatId, ActionMetier.ConsulterCv, Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new PermissionRefuseeException()));
        var handler = new ConsulterMonCvHandler(_permissions, _depot);

        Func<Task> action = () => handler.Handle(new ConsulterMonCvQuery(candidatId), CancellationToken.None);

        await action.Should().ThrowAsync<PermissionRefuseeException>();
        await _depot.DidNotReceive().ObtenirParCandidatAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SansCvConstitueLaConsultationRenvoieNull()
    {
        var candidatId = Guid.NewGuid();
        _depot.ObtenirParCandidatAsync(candidatId, Arg.Any<CancellationToken>())
              .Returns((CurriculumVitae?)null);
        var handler = new ConsulterMonCvHandler(_permissions, _depot);

        var resultat = await handler.Handle(new ConsulterMonCvQuery(candidatId), CancellationToken.None);

        resultat.Should().BeNull();
    }

    [Fact]
    public async Task LeCandidatRecupereSaPresentationEtSesCompetences()
    {
        var candidatId = Guid.NewGuid();
        var cv = CurriculumVitae.Constituer(candidatId, "Développeuse senior", new[] { "C#", "Azure" });
        _depot.ObtenirParCandidatAsync(candidatId, Arg.Any<CancellationToken>()).Returns(cv);
        var handler = new ConsulterMonCvHandler(_permissions, _depot);

        var resultat = await handler.Handle(new ConsulterMonCvQuery(candidatId), CancellationToken.None);

        resultat.Should().NotBeNull();
        resultat!.Id.Should().Be(cv.Id);
        resultat.Presentation.Should().Be("Développeuse senior");
        resultat.Competences.Should().BeEquivalentTo("C#", "Azure");
    }
}
