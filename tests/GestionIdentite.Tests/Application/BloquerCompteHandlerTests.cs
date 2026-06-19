using CVTech.Modules.GestionIdentite.Application;
using CVTech.Modules.GestionIdentite.Application.Features.BloquerCompte;
using CVTech.Modules.GestionIdentite.Contracts;
using CVTech.Modules.GestionIdentite.Domaine;
using CVTech.SharedKernel.Permissions;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace CVTech.Modules.GestionIdentite.Tests.Application;

public class BloquerCompteHandlerTests
{
    private readonly IVerificateurPermission _permissions = Substitute.For<IVerificateurPermission>();
    private readonly IDepotUtilisateurs _depot = Substitute.For<IDepotUtilisateurs>();

    [Fact]
    public async Task UnCandidatNePeutPasBloquerUnCompte()
    {
        // Arrange : le vérificateur refuse l'action (rôle non autorisé).
        var appelantId = Guid.NewGuid();
        _permissions
            .ExigerAsync(appelantId, ActionMetier.BloquerOuReactiverCompte, Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new PermissionRefuseeException()));
        var handler = new BloquerCompteHandler(_permissions, _depot);

        // Act
        Func<Task> action = () => handler.Handle(
            new BloquerCompteCommand(appelantId, Guid.NewGuid()), CancellationToken.None);

        // Assert : refus métier, et AUCUNE action de persistance déclenchée.
        await action.Should().ThrowAsync<PermissionRefuseeException>();
        await _depot.DidNotReceive().ObtenirAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UnAdministrateurPeutBloquerUnCompte()
    {
        // Arrange : permission accordée, la cible existe.
        var adminId = Guid.NewGuid();
        var cible = Utilisateur.Inscrire("candidat@cvtech.fr", RoleUtilisateur.Candidat);
        _permissions
            .ExigerAsync(adminId, ActionMetier.BloquerOuReactiverCompte, Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _depot.ObtenirAsync(cible.Id, Arg.Any<CancellationToken>()).Returns(cible);
        var handler = new BloquerCompteHandler(_permissions, _depot);

        // Act
        await handler.Handle(new BloquerCompteCommand(adminId, cible.Id), CancellationToken.None);

        // Assert
        cible.EstBloque.Should().BeTrue();
        await _depot.Received(1).MettreAJourAsync(cible, Arg.Any<CancellationToken>());
    }
}
