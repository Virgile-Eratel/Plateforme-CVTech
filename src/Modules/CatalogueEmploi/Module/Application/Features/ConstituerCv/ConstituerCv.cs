using CVTech.Modules.CatalogueEmploi.Domaine;
using CVTech.Modules.GestionIdentite.Contracts;
using FluentValidation;
using MediatR;

namespace CVTech.Modules.CatalogueEmploi.Application.Features.ConstituerCv;

public sealed record ConstituerCvCommand(
    Guid CandidatId,
    string Presentation,
    IReadOnlyList<string> Competences) : IRequest<Guid>;

public sealed class ConstituerCvValidator : AbstractValidator<ConstituerCvCommand>
{
    public ConstituerCvValidator()
    {
        RuleFor(c => c.CandidatId).NotEmpty();
        RuleFor(c => c.Presentation).NotEmpty();
    }
}

public sealed class ConstituerCvHandler(
    IVerificateurPermission permissions,
    IDepotCv depot) : IRequestHandler<ConstituerCvCommand, Guid>
{
    public async Task<Guid> Handle(ConstituerCvCommand commande, CancellationToken ct)
    {
        await permissions.ExigerAsync(commande.CandidatId, ActionMetier.ConstituerCv, ct: ct);

        var cv = CurriculumVitae.Constituer(commande.CandidatId, commande.Presentation, commande.Competences);
        await depot.AjouterAsync(cv, ct);
        await depot.EnregistrerAsync(ct);

        return cv.Id;
    }
}
