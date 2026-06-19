using CVTech.Modules.AppelOffreFreelance.Domaine;
using CVTech.Modules.AppelOffreFreelance.Infrastructure.Persistence.Entities;
using CVTech.SharedKernel.Domaine;

namespace CVTech.Modules.AppelOffreFreelance.Infrastructure.Persistence.Mappers;

/// <summary>Conversions entre l'agrégat <see cref="AppelOffre"/> et l'entité EF <see cref="AppelOffreEntity"/>.</summary>
public static class AppelOffreMapper
{
    public static AppelOffreEntity ToEntity(AppelOffre domaine) => new()
    {
        Id = domaine.Id,
        EntrepriseId = domaine.EntrepriseId,
        Titre = domaine.Titre,
        Statut = domaine.Statut,
        PropositionLaureateId = domaine.PropositionLaureateId,
        DatePublication = domaine.DatePublication,
        Contexte = domaine.CahierDesCharges.Contexte,
        Livrables = domaine.CahierDesCharges.Livrables,
        Deadline = domaine.CahierDesCharges.Deadline,
        BudgetMin = domaine.CahierDesCharges.BudgetMin,
        BudgetMax = domaine.CahierDesCharges.BudgetMax,
        DomaineCode = domaine.Domaine.Code,
        DomaineLibelle = domaine.Domaine.Libelle
    };

    public static AppelOffre ToDomain(AppelOffreEntity entite) =>
        AppelOffre.Reconstituer(
            entite.Id,
            entite.EntrepriseId,
            entite.Titre,
            CahierDesCharges.Reconstituer(
                entite.Contexte, entite.Livrables, entite.Deadline, entite.BudgetMin, entite.BudgetMax),
            DomaineMetier.Reconstituer(entite.DomaineCode, entite.DomaineLibelle),
            entite.Statut,
            entite.PropositionLaureateId,
            entite.DatePublication);

    /// <summary>Reporte l'état mutable de l'agrégat sur une entité déjà suivie (mise à jour).</summary>
    public static void Appliquer(AppelOffre domaine, AppelOffreEntity entite)
    {
        entite.EntrepriseId = domaine.EntrepriseId;
        entite.Titre = domaine.Titre;
        entite.Statut = domaine.Statut;
        entite.PropositionLaureateId = domaine.PropositionLaureateId;
        entite.DatePublication = domaine.DatePublication;
        entite.Contexte = domaine.CahierDesCharges.Contexte;
        entite.Livrables = domaine.CahierDesCharges.Livrables;
        entite.Deadline = domaine.CahierDesCharges.Deadline;
        entite.BudgetMin = domaine.CahierDesCharges.BudgetMin;
        entite.BudgetMax = domaine.CahierDesCharges.BudgetMax;
        entite.DomaineCode = domaine.Domaine.Code;
        entite.DomaineLibelle = domaine.Domaine.Libelle;
    }
}
