using CVTech.Modules.CatalogueEmploi.Application;
using CVTech.Modules.CatalogueEmploi.Application.Features.PublierAnnonce;
using CVTech.Modules.CatalogueEmploi.Contracts;
using CVTech.Modules.CatalogueEmploi.Domaine;
using CVTech.Modules.GestionIdentite.Contracts;
using CVTech.SharedKernel.Evenements;
using CVTech.SharedKernel.Permissions;
using FluentAssertions;
using NSubstitute;

namespace CVTech.Modules.CatalogueEmploi.Tests;

public class PublierAnnonceHandlerTests
{
    private readonly IVerificateurPermission _permissions = Substitute.For<IVerificateurPermission>();
    private readonly IDepotAnnonces _depot = Substitute.For<IDepotAnnonces>();
    private readonly IBusEvenements _bus = Substitute.For<IBusEvenements>();

    [Fact]
    public async Task UneEntreprisePublieUneAnnonceEtEmetLevenementAnnoncePubliee()
    {
        var entrepriseId = Guid.NewGuid();
        var handler = new PublierAnnonceHandler(_permissions, _depot, _bus);

        var id = await handler.Handle(
            new PublierAnnonceCommand(entrepriseId, "Dev .NET", "Mission cloud", TypeContrat.CDI, "Cloud Azure"),
            CancellationToken.None);

        await _bus.Received(1).PublierAsync(
            Arg.Is<AnnoncePubliee>(e => e.AnnonceId == id && e.DomaineCode == "cloud-azure"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UnCandidatNePeutPasPublierUneAnnonce()
    {
        var candidatId = Guid.NewGuid();
        _permissions
            .ExigerAsync(candidatId, ActionMetier.PublierAnnonce, Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new PermissionRefuseeException()));
        var handler = new PublierAnnonceHandler(_permissions, _depot, _bus);

        Func<Task> action = () => handler.Handle(
            new PublierAnnonceCommand(candidatId, "Dev", "x", TypeContrat.CDI, "Cloud Azure"),
            CancellationToken.None);

        await action.Should().ThrowAsync<PermissionRefuseeException>();
        await _bus.DidNotReceive().PublierAsync(Arg.Any<IEvenementIntegration>(), Arg.Any<CancellationToken>());
    }
}
