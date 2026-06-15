using CVTech.Modules.ActualiteEtAbonnement.Application.EventHandlers;
using CVTech.Modules.ActualiteEtAbonnement.Application;
using CVTech.Modules.ActualiteEtAbonnement.Domaine;
using CVTech.Modules.ActualiteEtAbonnement.Tests.Doubles;
using CVTech.Modules.CatalogueEmploi.Contracts;
using CVTech.SharedKernel.Domaine;
using FluentAssertions;
using NSubstitute;

namespace CVTech.Modules.ActualiteEtAbonnement.Tests;

public class NotificationParDomaineTests
{
    [Fact]
    public async Task SeulsLesAbonnésDuDomaineConcernéSontNotifiésÀLaPublication()
    {
        // Arrange : deux abonnés sur des domaines différents.
        var abonnements = new DepotAbonnementsFactice();
        var notifications = new DepotNotificationsFactice();
        var notificateur = Substitute.For<INotificateurTempsReel>();

        var abonneCloud = Guid.NewGuid();
        var abonneData = Guid.NewGuid();
        await abonnements.AjouterOuMettreAJourAsync(
            Abonnement.Creer(abonneCloud, [DomaineMetier.Creer("Cloud Azure")]));
        await abonnements.AjouterOuMettreAJourAsync(
            Abonnement.Creer(abonneData, [DomaineMetier.Creer("Data Science")]));

        var handler = new NotifierSurAnnoncePubliee(abonnements, notifications, notificateur);

        // Act : une annonce paraît dans « Cloud Azure ».
        await handler.Handle(
            new AnnoncePubliee(Guid.NewGuid(), "Ingénieur Cloud", Guid.NewGuid(), "cloud-azure", "Cloud Azure"),
            CancellationToken.None);

        // Assert : seul l'abonné Cloud est notifié.
        await notificateur.Received(1).PousserAsync(abonneCloud, Arg.Any<Notification>(), Arg.Any<CancellationToken>());
        await notificateur.DidNotReceive().PousserAsync(abonneData, Arg.Any<Notification>(), Arg.Any<CancellationToken>());

        var notifsCloud = await notifications.ListerParDestinataireAsync(abonneCloud);
        var notifsData = await notifications.ListerParDestinataireAsync(abonneData);
        notifsCloud.Should().HaveCount(1);
        notifsData.Should().BeEmpty();
    }
}
