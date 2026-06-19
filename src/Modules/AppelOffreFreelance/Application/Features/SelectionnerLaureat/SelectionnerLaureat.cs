using CVTech.Modules.AppelOffreFreelance.Domaine;
using CVTech.Modules.GestionIdentite.Contracts;
using CVTech.SharedKernel.Permissions;
using FluentValidation;
using MediatR;

namespace CVTech.Modules.AppelOffreFreelance.Application.Features.SelectionnerLaureat;

public sealed record SelectionnerLaureatCommand(
    Guid AppelantId,
    Guid AppelOffreId,
    Guid PropositionId) : IRequest;

public sealed class SelectionnerLaureatValidator : AbstractValidator<SelectionnerLaureatCommand>
{
    public SelectionnerLaureatValidator()
    {
        RuleFor(c => c.AppelantId).NotEmpty();
        RuleFor(c => c.AppelOffreId).NotEmpty();
        RuleFor(c => c.PropositionId).NotEmpty();
    }
}

public sealed class SelectionnerLaureatHandler(
    IVerificateurPermission permissions,
    IDepotAppelsOffre depotAppels,
    IDepotPropositions depotPropositions) : IRequestHandler<SelectionnerLaureatCommand>
{
    public async Task Handle(SelectionnerLaureatCommand commande, CancellationToken ct)
    {
        // 1. PERMISSION D'ABORD : consulter/traiter les propositions reçues.
        await permissions.ExigerAsync(commande.AppelantId, ActionMetier.ConsulterCandidaturesRecues, ct: ct);

        var appelOffre = await depotAppels.ObtenirAsync(commande.AppelOffreId, ct)
            ?? throw new InvalidOperationException("Appel d'offre introuvable.");

        // 2. Règle de propriété : seul le propriétaire de l'AO sélectionne son lauréat.
        if (appelOffre.EntrepriseId != commande.AppelantId)
            throw new PermissionRefuseeException("Seule l'entreprise propriétaire peut sélectionner le lauréat.");

        var proposition = await depotPropositions.ObtenirAsync(commande.PropositionId, ct)
            ?? throw new InvalidOperationException("Proposition introuvable.");
        if (proposition.AppelOffreId != appelOffre.Id)
            throw new InvalidOperationException("Cette proposition ne concerne pas cet appel d'offre.");

        appelOffre.SelectionnerLaureat(proposition.Id);
        await depotAppels.EnregistrerAsync(ct);
    }
}
