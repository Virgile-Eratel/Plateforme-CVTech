using CVTech.Modules.GestionIdentite.Domaine;
using MediatR;

namespace CVTech.Modules.GestionIdentite.Application.Features.InscrireUtilisateur;

/// <summary>Inscrit un nouvel utilisateur. Action publique (pas de permission requise).</summary>
public sealed record InscrireUtilisateurCommand(string Email, string MotDePasse, RoleUtilisateur Role)
    : IRequest<InscriptionResultat>;

/// <summary>
/// Résultat autoritatif de l'inscription : identité et rôle réellement persistés. Le jeton
/// doit être généré à partir de ce rôle, jamais du rôle brut de la requête.
/// </summary>
public sealed record InscriptionResultat(Guid Id, RoleUtilisateur Role);
