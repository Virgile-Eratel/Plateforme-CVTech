using CVTech.Modules.CatalogueEmploi.Contracts;
using CVTech.Modules.CatalogueEmploi.Domaine;
using CVTech.Modules.GestionIdentite.Contracts;
using CVTech.SharedKernel.Domaine;
using CVTech.SharedKernel.Evenements;
using FluentValidation;
using MediatR;

namespace CVTech.Modules.CatalogueEmploi.Application.Features.PublierAnnonce;

public sealed record PublierAnnonceCommand(
    Guid EntrepriseId,
    string Titre,
    string Description,
    TypeContrat TypeContrat,
    string DomaineLibelle) : IRequest<Guid>;

public sealed class PublierAnnonceValidator : AbstractValidator<PublierAnnonceCommand>
{
    public PublierAnnonceValidator()
    {
        RuleFor(c => c.EntrepriseId).NotEmpty();
        RuleFor(c => c.Titre).NotEmpty().MaximumLength(200);
        RuleFor(c => c.TypeContrat).IsInEnum();
        RuleFor(c => c.DomaineLibelle).NotEmpty();
    }
}

public sealed class PublierAnnonceHandler(
    IVerificateurPermission permissions,
    IDepotAnnonces depot,
    IBusEvenements bus) : IRequestHandler<PublierAnnonceCommand, Guid>
{
    public async Task<Guid> Handle(PublierAnnonceCommand commande, CancellationToken ct)
    {
        // 1. PERMISSION D'ABORD.
        await permissions.ExigerAsync(commande.EntrepriseId, ActionMetier.PublierAnnonce, ct: ct);

        // 2. Action métier.
        var domaine = DomaineMetier.Creer(commande.DomaineLibelle);
        var annonce = AnnonceEmploi.Publier(
            commande.EntrepriseId, commande.Titre, commande.Description, commande.TypeContrat, domaine);

        await depot.AjouterAsync(annonce, ct);
        await depot.EnregistrerAsync(ct);

        // 3. Publication de l'événement sur le bus interne (ADR 0003).
        await bus.PublierAsync(
            new AnnoncePubliee(annonce.Id, annonce.Titre, annonce.EntrepriseId, domaine.Code, domaine.Libelle),
            ct);

        return annonce.Id;
    }
}
