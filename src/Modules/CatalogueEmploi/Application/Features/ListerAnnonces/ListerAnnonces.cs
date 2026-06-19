using CVTech.Modules.CatalogueEmploi.Application;
using MediatR;

using CVTech.Modules.CatalogueEmploi.Domaine;

namespace CVTech.Modules.CatalogueEmploi.Application.Features.ListerAnnonces;

/// <summary>Consultation publique des annonces : action anonyme, sans permission.</summary>
public sealed record ListerAnnoncesQuery(string? DomaineCode = null) : IRequest<IReadOnlyList<AnnonceVue>>;

public sealed record AnnonceVue(
    Guid Id,
    string Titre,
    string Description,
    string TypeContrat,
    string Domaine,
    DateTimeOffset DatePublication);

public sealed class ListerAnnoncesHandler(IDepotAnnonces depot)
    : IRequestHandler<ListerAnnoncesQuery, IReadOnlyList<AnnonceVue>>
{
    public async Task<IReadOnlyList<AnnonceVue>> Handle(ListerAnnoncesQuery requete, CancellationToken ct)
    {
        var annonces = await depot.ListerAsync(requete.DomaineCode, ct);
        return annonces
            .Select(a => new AnnonceVue(
                a.Id, a.Titre, a.Description, a.TypeContrat.ToString(), a.Domaine.Libelle, a.DatePublication))
            .ToList();
    }
}
