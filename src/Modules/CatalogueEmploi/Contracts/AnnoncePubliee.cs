using CVTech.SharedKernel.Evenements;

namespace CVTech.Modules.CatalogueEmploi.Contracts;

/// <summary>
/// Événement d'intégration émis à la publication d'une annonce d'emploi.
/// Consommé par ActualiteEtAbonnement pour notifier les abonnés du domaine.
/// </summary>
public sealed record AnnoncePubliee(
    Guid AnnonceId,
    string Titre,
    Guid EntrepriseId,
    string DomaineCode,
    string DomaineLibelle) : IEvenementIntegration;
