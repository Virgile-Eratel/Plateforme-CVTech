namespace CVTech.Modules.GestionIdentite.Domaine;

/// <summary>
/// Exception métier : un rôle ne peut pas être obtenu par auto-inscription publique.
/// Un compte administrateur n'est jamais créé via le parcours d'inscription ; il est
/// provisionné par l'administration (cf. <see cref="Utilisateur.Creer"/>).
/// </summary>
public sealed class RoleInscriptionInterditException(RoleUtilisateur role)
    : Exception($"Le rôle « {role} » ne peut pas être obtenu par inscription publique.");
