using CVTech.Modules.AppelOffreFreelance.Domaine;
using CVTech.Modules.AppelOffreFreelance.Infrastructure.Persistence.Entities;

namespace CVTech.Modules.AppelOffreFreelance.Infrastructure.Persistence.Mappers;

/// <summary>Conversions entre l'agrégat <see cref="PropositionFreelance"/> et l'entité EF <see cref="PropositionFreelanceEntity"/>.</summary>
public static class PropositionFreelanceMapper
{
    public static PropositionFreelanceEntity ToEntity(PropositionFreelance domaine) => new()
    {
        Id = domaine.Id,
        AppelOffreId = domaine.AppelOffreId,
        CandidatId = domaine.CandidatId,
        DureeJours = domaine.DureeJours,
        Methodologie = domaine.Methodologie,
        DateSoumission = domaine.DateSoumission,
        Tjm = domaine.Tjm.MontantJournalier
    };

    public static PropositionFreelance ToDomain(PropositionFreelanceEntity entite) =>
        PropositionFreelance.Reconstituer(
            entite.Id,
            entite.AppelOffreId,
            entite.CandidatId,
            BaremeTJM.Reconstituer(entite.Tjm),
            entite.DureeJours,
            entite.Methodologie,
            entite.DateSoumission);

    /// <summary>Reporte l'état mutable de l'agrégat sur une entité déjà suivie (mise à jour).</summary>
    public static void Appliquer(PropositionFreelance domaine, PropositionFreelanceEntity entite)
    {
        entite.AppelOffreId = domaine.AppelOffreId;
        entite.CandidatId = domaine.CandidatId;
        entite.DureeJours = domaine.DureeJours;
        entite.Methodologie = domaine.Methodologie;
        entite.DateSoumission = domaine.DateSoumission;
        entite.Tjm = domaine.Tjm.MontantJournalier;
    }
}
