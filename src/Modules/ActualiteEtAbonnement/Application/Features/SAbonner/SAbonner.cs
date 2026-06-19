using CVTech.Modules.ActualiteEtAbonnement.Domaine;
using CVTech.Modules.GestionIdentite.Contracts;
using CVTech.SharedKernel.Domaine;
using FluentValidation;
using MediatR;

namespace CVTech.Modules.ActualiteEtAbonnement.Application.Features.SAbonner;

public sealed record SAbonnerCommand(
    Guid UtilisateurId,
    IReadOnlyList<string> DomainesLibelles,
    CanalDiffusion Canal = CanalDiffusion.InApp) : IRequest;

public sealed class SAbonnerValidator : AbstractValidator<SAbonnerCommand>
{
    public SAbonnerValidator()
    {
        RuleFor(c => c.UtilisateurId).NotEmpty();
        RuleFor(c => c.DomainesLibelles).NotEmpty();
    }
}

public sealed class SAbonnerHandler(
    IVerificateurPermission permissions,
    IDepotAbonnements depot) : IRequestHandler<SAbonnerCommand>
{
    public async Task Handle(SAbonnerCommand commande, CancellationToken ct)
    {
        await permissions.ExigerAsync(commande.UtilisateurId, ActionMetier.SAbonnerDomaine, ct: ct);

        var domaines = commande.DomainesLibelles
            .Where(d => !string.IsNullOrWhiteSpace(d))
            .Select(DomaineMetier.Creer)
            .ToList();

        var abonnement = await depot.ObtenirParUtilisateurAsync(commande.UtilisateurId, ct);
        if (abonnement is null)
            abonnement = Abonnement.Creer(commande.UtilisateurId, domaines, commande.Canal);
        else
            abonnement.AjouterDomaines(domaines);

        await depot.AjouterOuMettreAJourAsync(abonnement, ct);
    }
}
