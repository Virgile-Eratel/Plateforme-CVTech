using CVTech.Modules.GestionIdentite.Contracts;
using MediatR;

using CVTech.Modules.CatalogueEmploi.Domaine;

namespace CVTech.Modules.CatalogueEmploi.Application.Features.ConsulterMonCv;

/// <summary>Lecture par le candidat de SON propre CV. Renvoie null s'il n'en a pas encore constitué.</summary>
public sealed record ConsulterMonCvQuery(Guid CandidatId) : IRequest<CvVue?>;

public sealed record CvVue(Guid Id, string Presentation, IReadOnlyList<string> Competences);

public sealed class ConsulterMonCvHandler(
    IVerificateurPermission permissions,
    IDepotCv depot) : IRequestHandler<ConsulterMonCvQuery, CvVue?>
{
    public async Task<CvVue?> Handle(ConsulterMonCvQuery requete, CancellationToken ct)
    {
        await permissions.ExigerAsync(requete.CandidatId, ActionMetier.ConsulterCv, ct: ct);

        var cv = await depot.ObtenirParCandidatAsync(requete.CandidatId, ct);
        return cv is null
            ? null
            : new CvVue(cv.Id, cv.Presentation, cv.Competences.ToList());
    }
}
