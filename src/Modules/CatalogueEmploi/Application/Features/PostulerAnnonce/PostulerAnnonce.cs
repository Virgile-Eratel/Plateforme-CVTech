using CVTech.Modules.CatalogueEmploi.Domaine;
using CVTech.Modules.GestionIdentite.Contracts;
using FluentValidation;
using MediatR;

namespace CVTech.Modules.CatalogueEmploi.Application.Features.PostulerAnnonce;

public sealed record PostulerAnnonceCommand(
    Guid CandidatId,
    Guid AnnonceId,
    string? LettreMotivation) : IRequest<Guid>;

public sealed class PostulerAnnonceValidator : AbstractValidator<PostulerAnnonceCommand>
{
    public PostulerAnnonceValidator()
    {
        RuleFor(c => c.CandidatId).NotEmpty();
        RuleFor(c => c.AnnonceId).NotEmpty();
    }
}

public sealed class PostulerAnnonceHandler(
    IVerificateurPermission permissions,
    IDepotAnnonces depotAnnonces,
    IDepotCandidatures depotCandidatures) : IRequestHandler<PostulerAnnonceCommand, Guid>
{
    public async Task<Guid> Handle(PostulerAnnonceCommand commande, CancellationToken ct)
    {
        // 1. PERMISSION D'ABORD (un compte bloqué ou non-candidat est refusé ici).
        await permissions.ExigerAsync(commande.CandidatId, ActionMetier.PostulerAnnonce, ct: ct);

        // 2. Action métier.
        var annonce = await depotAnnonces.ObtenirAsync(commande.AnnonceId, ct)
            ?? throw new InvalidOperationException("Annonce introuvable.");

        var candidature = Candidature.Deposer(annonce.Id, commande.CandidatId, commande.LettreMotivation);
        await depotCandidatures.AjouterAsync(candidature, ct);

        return candidature.Id;
    }
}
