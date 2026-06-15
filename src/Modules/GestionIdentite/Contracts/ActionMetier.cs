namespace CVTech.Modules.GestionIdentite.Contracts;

/// <summary>
/// Actions protégées de la plateforme, traduites directement de la matrice de
/// permissions du README. Les consultations publiques (annonces, AO, RSS) n'y
/// figurent pas : elles sont anonymes.
/// </summary>
public enum ActionMetier
{
    ConstituerCv,
    PostulerAnnonce,
    SoumettreProposition,
    PublierAnnonce,
    PublierAppelOffre,
    ConsulterCandidaturesRecues,
    SAbonnerDomaine,
    PublierArticleActualite,
    ModererAnnonceOuAppelOffre,
    BloquerOuReactiverCompte,
    GererReferentielDomaines
}
