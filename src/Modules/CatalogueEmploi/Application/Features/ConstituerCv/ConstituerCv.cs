using CVTech.Modules.CatalogueEmploi.Domaine;
using CVTech.Modules.GestionIdentite.Contracts;
using FluentValidation;
using MediatR;

namespace CVTech.Modules.CatalogueEmploi.Application.Features.ConstituerCv;

public sealed record ConstituerCvCommand(
    Guid CandidatId,
    string Presentation,
    IReadOnlyList<string> Competences,
    int? Age = null) : IRequest<Guid>;

public sealed class ConstituerCvValidator : AbstractValidator<ConstituerCvCommand>
{
    public ConstituerCvValidator()
    {
        RuleFor(c => c.CandidatId).NotEmpty();
        RuleFor(c => c.Presentation).NotEmpty();
        // Âge optionnel ; la fourchette métier reste garantie par l'agrégat (16–100 ans).
        RuleFor(c => c.Age!.Value).InclusiveBetween(16, 100).When(c => c.Age.HasValue);
    }
}

public sealed class ConstituerCvHandler(
    IVerificateurPermission permissions,
    IDepotCv depot) : IRequestHandler<ConstituerCvCommand, Guid>
{
    public async Task<Guid> Handle(ConstituerCvCommand commande, CancellationToken ct)
    {
        await permissions.ExigerAsync(commande.CandidatId, ActionMetier.ConstituerCv, ct: ct);

        // Un seul CV par candidat : on révise l'existant plutôt que d'en accumuler de nouveaux.
        var cv = await depot.ObtenirParCandidatAsync(commande.CandidatId, ct);
        if (cv is null)
        {
            cv = CurriculumVitae.Constituer(
                commande.CandidatId, commande.Presentation, commande.Competences, commande.Age);
            await depot.AjouterAsync(cv, ct);
        }
        else
        {
            cv.MettreAJour(commande.Presentation, commande.Competences, commande.Age);
            await depot.MettreAJourAsync(cv, ct);
        }

        return cv.Id;
    }
}
