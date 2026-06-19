using CVTech.Modules.AppelOffreFreelance.Domaine;
using CVTech.Modules.GestionIdentite.Contracts;
using FluentValidation;
using MediatR;

namespace CVTech.Modules.AppelOffreFreelance.Application.Features.SoumettreProposition;

public sealed record SoumettrePropositionCommand(
    Guid CandidatId,
    Guid AppelOffreId,
    decimal MontantTJM,
    int DureeJours,
    string Methodologie) : IRequest<Guid>;

public sealed class SoumettrePropositionValidator : AbstractValidator<SoumettrePropositionCommand>
{
    public SoumettrePropositionValidator()
    {
        RuleFor(c => c.CandidatId).NotEmpty();
        RuleFor(c => c.AppelOffreId).NotEmpty();
        RuleFor(c => c.MontantTJM).GreaterThan(0);
        RuleFor(c => c.DureeJours).GreaterThan(0);
    }
}

public sealed class SoumettrePropositionHandler(
    IVerificateurPermission permissions,
    IDepotAppelsOffre depotAppels,
    IDepotPropositions depotPropositions) : IRequestHandler<SoumettrePropositionCommand, Guid>
{
    public async Task<Guid> Handle(SoumettrePropositionCommand commande, CancellationToken ct)
    {
        await permissions.ExigerAsync(commande.CandidatId, ActionMetier.SoumettreProposition, ct: ct);

        var appelOffre = await depotAppels.ObtenirAsync(commande.AppelOffreId, ct)
            ?? throw new InvalidOperationException("Appel d'offre introuvable.");

        var proposition = PropositionFreelance.Soumettre(
            appelOffre.Id,
            commande.CandidatId,
            BaremeTJM.Creer(commande.MontantTJM),
            commande.DureeJours,
            commande.Methodologie);

        await depotPropositions.AjouterAsync(proposition, ct);

        return proposition.Id;
    }
}
