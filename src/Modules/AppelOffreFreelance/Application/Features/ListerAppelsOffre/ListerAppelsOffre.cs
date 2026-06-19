using MediatR;

using CVTech.Modules.AppelOffreFreelance.Domaine;

namespace CVTech.Modules.AppelOffreFreelance.Application.Features.ListerAppelsOffre;

/// <summary>Consultation publique des appels d'offre : action anonyme, sans permission.</summary>
public sealed record ListerAppelsOffreQuery(string? DomaineCode = null) : IRequest<IReadOnlyList<AppelOffreVue>>;

public sealed record AppelOffreVue(
    Guid Id,
    string Titre,
    string Contexte,
    string Livrables,
    DateTimeOffset Deadline,
    decimal BudgetMin,
    decimal BudgetMax,
    string Domaine,
    string Statut,
    DateTimeOffset DatePublication);

public sealed class ListerAppelsOffreHandler(IDepotAppelsOffre depot)
    : IRequestHandler<ListerAppelsOffreQuery, IReadOnlyList<AppelOffreVue>>
{
    public async Task<IReadOnlyList<AppelOffreVue>> Handle(ListerAppelsOffreQuery requete, CancellationToken ct)
    {
        var appels = await depot.ListerAsync(requete.DomaineCode, ct);
        return appels.Select(a => new AppelOffreVue(
            a.Id, a.Titre, a.CahierDesCharges.Contexte, a.CahierDesCharges.Livrables,
            a.CahierDesCharges.Deadline, a.CahierDesCharges.BudgetMin, a.CahierDesCharges.BudgetMax,
            a.Domaine.Libelle, a.Statut.ToString(), a.DatePublication)).ToList();
    }
}
