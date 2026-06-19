using CVTech.Modules.AppelOffreFreelance.Contracts;
using CVTech.Modules.AppelOffreFreelance.Domaine;
using CVTech.Modules.GestionIdentite.Contracts;
using CVTech.SharedKernel.Domaine;
using CVTech.SharedKernel.Evenements;
using FluentValidation;
using MediatR;

namespace CVTech.Modules.AppelOffreFreelance.Application.Features.PublierAppelOffre;

public sealed record PublierAppelOffreCommand(
    Guid EntrepriseId,
    string Titre,
    string Contexte,
    string Livrables,
    DateTimeOffset Deadline,
    decimal BudgetMin,
    decimal BudgetMax,
    string DomaineLibelle) : IRequest<Guid>;

public sealed class PublierAppelOffreValidator : AbstractValidator<PublierAppelOffreCommand>
{
    public PublierAppelOffreValidator()
    {
        RuleFor(c => c.EntrepriseId).NotEmpty();
        RuleFor(c => c.Titre).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Contexte).NotEmpty();
        RuleFor(c => c.Livrables).NotEmpty();
        RuleFor(c => c.DomaineLibelle).NotEmpty();
        RuleFor(c => c.BudgetMax).GreaterThanOrEqualTo(c => c.BudgetMin);
    }
}

public sealed class PublierAppelOffreHandler(
    IVerificateurPermission permissions,
    IDepotAppelsOffre depot,
    IBusEvenements bus) : IRequestHandler<PublierAppelOffreCommand, Guid>
{
    public async Task<Guid> Handle(PublierAppelOffreCommand commande, CancellationToken ct)
    {
        await permissions.ExigerAsync(commande.EntrepriseId, ActionMetier.PublierAppelOffre, ct: ct);

        var domaine = DomaineMetier.Creer(commande.DomaineLibelle);
        var cahier = CahierDesCharges.Creer(
            commande.Contexte, commande.Livrables, commande.Deadline, commande.BudgetMin, commande.BudgetMax);
        var appelOffre = AppelOffre.Publier(commande.EntrepriseId, commande.Titre, cahier, domaine);

        await depot.AjouterAsync(appelOffre, ct);
        await depot.EnregistrerAsync(ct);

        await bus.PublierAsync(
            new AppelOffrePublie(
                appelOffre.Id, appelOffre.Titre, appelOffre.EntrepriseId, domaine.Code, domaine.Libelle),
            ct);

        return appelOffre.Id;
    }
}
