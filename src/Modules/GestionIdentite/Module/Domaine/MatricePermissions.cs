using CVTech.Modules.GestionIdentite.Contracts;

namespace CVTech.Modules.GestionIdentite.Domaine;

/// <summary>
/// Source de vérité des droits : traduit la matrice de permissions du README.
/// Seul GestionIdentite la connaît ; les autres modules passent par
/// <see cref="IVerificateurPermission"/>.
/// </summary>
public static class MatricePermissions
{
    private static readonly Dictionary<RoleUtilisateur, HashSet<ActionMetier>> Autorisations = new()
    {
        [RoleUtilisateur.Candidat] =
        [
            ActionMetier.ConstituerCv,
            ActionMetier.PostulerAnnonce,
            ActionMetier.SoumettreProposition,
            ActionMetier.SAbonnerDomaine
        ],
        [RoleUtilisateur.Entreprise] =
        [
            ActionMetier.PublierAnnonce,
            ActionMetier.PublierAppelOffre,
            ActionMetier.ConsulterCandidaturesRecues,
            ActionMetier.SAbonnerDomaine
        ],
        [RoleUtilisateur.Administrateur] =
        [
            // L'administrateur hérite de tous les droits.
            .. Enum.GetValues<ActionMetier>()
        ]
    };

    public static bool EstAutorise(RoleUtilisateur role, ActionMetier action) =>
        Autorisations.TryGetValue(role, out var actions) && actions.Contains(action);
}
