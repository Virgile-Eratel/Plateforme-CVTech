using CVTech.Modules.AppelOffreFreelance.Domaine;

namespace CVTech.Modules.AppelOffreFreelance.Infrastructure.Persistence.Entities;

/// <summary>
/// Entité de persistance EF Core (DAO plat) de l'agrégat <see cref="PropositionFreelance"/>.
/// Aucune logique métier : value object BaremeTJM aplati en colonne simple.
/// </summary>
public sealed class PropositionFreelanceEntity
{
    public Guid Id { get; set; }
    public Guid AppelOffreId { get; set; }
    public Guid CandidatId { get; set; }
    public int DureeJours { get; set; }
    public string Methodologie { get; set; } = default!;
    public DateTimeOffset DateSoumission { get; set; }

    // Value object BaremeTJM (aplati).
    public decimal Tjm { get; set; }
}
