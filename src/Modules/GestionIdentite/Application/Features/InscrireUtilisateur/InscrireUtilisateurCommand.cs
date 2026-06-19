using CVTech.Modules.GestionIdentite.Domaine;
using MediatR;

namespace CVTech.Modules.GestionIdentite.Application.Features.InscrireUtilisateur;

/// <summary>Inscrit un nouvel utilisateur. Action publique (pas de permission requise).</summary>
public sealed record InscrireUtilisateurCommand(string Email, string MotDePasse, RoleUtilisateur Role)
    : IRequest<Guid>;
