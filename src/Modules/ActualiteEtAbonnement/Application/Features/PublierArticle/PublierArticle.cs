using CVTech.Modules.ActualiteEtAbonnement.Domaine;
using CVTech.Modules.GestionIdentite.Contracts;
using CVTech.SharedKernel.Domaine;
using FluentValidation;
using MediatR;

namespace CVTech.Modules.ActualiteEtAbonnement.Application.Features.PublierArticle;

public sealed record PublierArticleCommand(
    Guid AuteurId,
    string Titre,
    string Contenu,
    CategorieEditoriale Categorie,
    string? DomaineLibelle = null,
    string? SourceNom = null,
    string? SourceUrl = null) : IRequest<Guid>;

public sealed class PublierArticleValidator : AbstractValidator<PublierArticleCommand>
{
    public PublierArticleValidator()
    {
        RuleFor(c => c.AuteurId).NotEmpty();
        RuleFor(c => c.Titre).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Contenu).NotEmpty();
        RuleFor(c => c.Categorie).IsInEnum();
    }
}

public sealed class PublierArticleHandler(
    IVerificateurPermission permissions,
    IDepotArticles depot) : IRequestHandler<PublierArticleCommand, Guid>
{
    public async Task<Guid> Handle(PublierArticleCommand commande, CancellationToken ct)
    {
        // Seul l'Administrateur publie dans le fil d'actualité.
        await permissions.ExigerAsync(commande.AuteurId, ActionMetier.PublierArticleActualite, ct: ct);

        var domaine = string.IsNullOrWhiteSpace(commande.DomaineLibelle)
            ? null
            : DomaineMetier.Creer(commande.DomaineLibelle);
        var source = string.IsNullOrWhiteSpace(commande.SourceNom)
            ? null
            : SourceExterne.Creer(commande.SourceNom, commande.SourceUrl ?? string.Empty);

        var article = ArticleActualite.Publier(
            commande.AuteurId, commande.Titre, commande.Contenu, commande.Categorie, domaine, source);

        await depot.AjouterAsync(article, ct);
        await depot.EnregistrerAsync(ct);

        return article.Id;
    }
}
