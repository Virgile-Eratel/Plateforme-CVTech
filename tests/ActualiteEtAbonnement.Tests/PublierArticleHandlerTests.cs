using CVTech.Modules.ActualiteEtAbonnement.Application;
using CVTech.Modules.ActualiteEtAbonnement.Application.Features.PublierArticle;
using CVTech.Modules.ActualiteEtAbonnement.Domaine;
using CVTech.Modules.GestionIdentite.Contracts;
using CVTech.SharedKernel.Permissions;
using FluentAssertions;
using NSubstitute;

namespace CVTech.Modules.ActualiteEtAbonnement.Tests;

public class PublierArticleHandlerTests
{
    private readonly IVerificateurPermission _permissions = Substitute.For<IVerificateurPermission>();
    private readonly IDepotArticles _depot = Substitute.For<IDepotArticles>();

    [Fact]
    public async Task UnNonAdministrateurNePeutPasPublierUnArticle()
    {
        var candidatId = Guid.NewGuid();
        _permissions
            .ExigerAsync(candidatId, ActionMetier.PublierArticleActualite, Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new PermissionRefuseeException()));
        var handler = new PublierArticleHandler(_permissions, _depot);

        Func<Task> action = () => handler.Handle(
            new PublierArticleCommand(candidatId, "Titre", "Contenu", CategorieEditoriale.Frameworks),
            CancellationToken.None);

        await action.Should().ThrowAsync<PermissionRefuseeException>();
        await _depot.DidNotReceive().AjouterAsync(Arg.Any<ArticleActualite>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UnAdministrateurPublieUnArticle()
    {
        var adminId = Guid.NewGuid();
        var handler = new PublierArticleHandler(_permissions, _depot);

        var id = await handler.Handle(
            new PublierArticleCommand(adminId, "Nouveau framework", "Contenu", CategorieEditoriale.Frameworks),
            CancellationToken.None);

        id.Should().NotBe(Guid.Empty);
        await _depot.Received(1).AjouterAsync(Arg.Any<ArticleActualite>(), Arg.Any<CancellationToken>());
    }
}
