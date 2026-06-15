using CVTech.SharedKernel.Evenements;

namespace CVTech.Modules.AppelOffreFreelance.Contracts;

/// <summary>
/// Événement d'intégration émis à la publication d'un appel d'offre.
/// Consommé par ActualiteEtAbonnement pour notifier les abonnés du domaine.
/// </summary>
public sealed record AppelOffrePublie(
    Guid AppelOffreId,
    string Titre,
    Guid EntrepriseId,
    string DomaineCode,
    string DomaineLibelle) : IEvenementIntegration;
