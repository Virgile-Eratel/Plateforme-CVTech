using CVTech.Modules.CatalogueEmploi.Application;
using CVTech.Modules.CatalogueEmploi.Application.Features.PostulerAnnonce;
using CVTech.Modules.CatalogueEmploi.Domaine;
using CVTech.Modules.GestionIdentite.Contracts;
using CVTech.SharedKernel.Domaine;
using CVTech.SharedKernel.Permissions;
using FluentAssertions;
using NSubstitute;

namespace CVTech.Modules.CatalogueEmploi.Tests;

public class PostulerAnnonceHandlerTests
{
    private readonly IVerificateurPermission _permissions = Substitute.For<IVerificateurPermission>();
    private readonly IDepotAnnonces _depotAnnonces = Substitute.For<IDepotAnnonces>();
    private readonly IDepotCandidatures _depotCandidatures = Substitute.For<IDepotCandidatures>();

    [Fact]
    public async Task UnCandidatBloquéNePeutPasPostuler()
    {
        var candidatId = Guid.NewGuid();
        _permissions
            .ExigerAsync(candidatId, ActionMetier.PostulerAnnonce, Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new PermissionRefuseeException()));
        var handler = new PostulerAnnonceHandler(_permissions, _depotAnnonces, _depotCandidatures);

        Func<Task> action = () => handler.Handle(
            new PostulerAnnonceCommand(candidatId, Guid.NewGuid(), null), CancellationToken.None);

        await action.Should().ThrowAsync<PermissionRefuseeException>();
        await _depotCandidatures.DidNotReceive().AjouterAsync(Arg.Any<Candidature>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UnCandidatAutoriséPeutPostulerSansLettreDeMotivation()
    {
        var candidatId = Guid.NewGuid();
        var annonce = AnnonceEmploi.Publier(
            Guid.NewGuid(), "Dev", "x", TypeContrat.CDI, DomaineMetier.Creer("Cloud Azure"));
        _depotAnnonces.ObtenirAsync(annonce.Id, Arg.Any<CancellationToken>()).Returns(annonce);
        var handler = new PostulerAnnonceHandler(_permissions, _depotAnnonces, _depotCandidatures);

        var id = await handler.Handle(
            new PostulerAnnonceCommand(candidatId, annonce.Id, null), CancellationToken.None);

        id.Should().NotBe(Guid.Empty);
        await _depotCandidatures.Received(1).AjouterAsync(
            Arg.Is<Candidature>(c => c.AnnonceId == annonce.Id && c.LettreMotivation == null),
            Arg.Any<CancellationToken>());
    }
}
