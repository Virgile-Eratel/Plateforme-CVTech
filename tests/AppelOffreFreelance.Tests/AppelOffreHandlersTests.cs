using CVTech.Modules.AppelOffreFreelance.Application;
using CVTech.Modules.AppelOffreFreelance.Application.Features.PublierAppelOffre;
using CVTech.Modules.AppelOffreFreelance.Application.Features.SelectionnerLaureat;
using CVTech.Modules.AppelOffreFreelance.Contracts;
using CVTech.Modules.AppelOffreFreelance.Domaine;
using CVTech.Modules.GestionIdentite.Contracts;
using CVTech.SharedKernel.Domaine;
using CVTech.SharedKernel.Evenements;
using CVTech.SharedKernel.Permissions;
using FluentAssertions;
using NSubstitute;

namespace CVTech.Modules.AppelOffreFreelance.Tests;

public class AppelOffreHandlersTests
{
    private readonly IVerificateurPermission _permissions = Substitute.For<IVerificateurPermission>();
    private readonly IDepotAppelsOffre _depotAppels = Substitute.For<IDepotAppelsOffre>();
    private readonly IDepotPropositions _depotPropositions = Substitute.For<IDepotPropositions>();
    private readonly IBusEvenements _bus = Substitute.For<IBusEvenements>();

    [Fact]
    public async Task UneEntreprisePublieUnAppelOffreEtÉmetLévénementAppelOffrePublie()
    {
        var entrepriseId = Guid.NewGuid();
        var handler = new PublierAppelOffreHandler(_permissions, _depotAppels, _bus);

        var id = await handler.Handle(new PublierAppelOffreCommand(
            entrepriseId, "Refonte API", "contexte", "livrables",
            DateTimeOffset.UtcNow.AddDays(30), 10000m, 20000m, "Cloud Azure"), CancellationToken.None);

        await _bus.Received(1).PublierAsync(
            Arg.Is<AppelOffrePublie>(e => e.AppelOffreId == id && e.DomaineCode == "cloud-azure"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UnCandidatNePeutPasPublierUnAppelOffre()
    {
        var candidatId = Guid.NewGuid();
        _permissions
            .ExigerAsync(candidatId, ActionMetier.PublierAppelOffre, Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new PermissionRefuseeException()));
        var handler = new PublierAppelOffreHandler(_permissions, _depotAppels, _bus);

        Func<Task> action = () => handler.Handle(new PublierAppelOffreCommand(
            candidatId, "x", "c", "l", DateTimeOffset.UtcNow.AddDays(5), 1m, 2m, "Cloud Azure"),
            CancellationToken.None);

        await action.Should().ThrowAsync<PermissionRefuseeException>();
    }

    [Fact]
    public async Task SeuleLentrepriseProprietairePeutSelectionnerLeLaureat()
    {
        var proprietaireId = Guid.NewGuid();
        var intrusId = Guid.NewGuid();
        var ao = AppelOffre.Publier(proprietaireId, "Mission",
            CahierDesCharges.Creer("c", "l", DateTimeOffset.UtcNow.AddDays(10), 0m, 100m),
            DomaineMetier.Creer("DevOps"));
        _depotAppels.ObtenirAsync(ao.Id, Arg.Any<CancellationToken>()).Returns(ao);
        var handler = new SelectionnerLaureatHandler(_permissions, _depotAppels, _depotPropositions);

        // L'intrus a la permission générique mais n'est pas le propriétaire de l'AO.
        Func<Task> action = () => handler.Handle(
            new SelectionnerLaureatCommand(intrusId, ao.Id, Guid.NewGuid()), CancellationToken.None);

        await action.Should().ThrowAsync<PermissionRefuseeException>();
        ao.Statut.Should().Be(StatutAppelOffre.Ouvert);
    }
}
