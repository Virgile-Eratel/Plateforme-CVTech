using CVTech.Modules.AppelOffreFreelance.Domaine;

namespace CVTech.Modules.AppelOffreFreelance.Infrastructure.Persistence.Entities;

/// <summary>
/// Entité de persistance EF Core (DAO plat) de l'agrégat <see cref="AppelOffre"/>.
/// Aucune logique métier : value objects aplatis en colonnes simples.
/// </summary>
public sealed class AppelOffreEntity
{
    public Guid Id { get; set; }
    public Guid EntrepriseId { get; set; }
    public string Titre { get; set; } = default!;
    public StatutAppelOffre Statut { get; set; }
    public Guid? PropositionLaureateId { get; set; }
    public DateTimeOffset DatePublication { get; set; }

    // Value object CahierDesCharges (aplati).
    public string Contexte { get; set; } = default!;
    public string Livrables { get; set; } = default!;
    public DateTimeOffset Deadline { get; set; }
    public decimal BudgetMin { get; set; }
    public decimal BudgetMax { get; set; }

    // Value object partagé DomaineMetier (aplati).
    public string DomaineCode { get; set; } = default!;
    public string DomaineLibelle { get; set; } = default!;
}
