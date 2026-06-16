using CVTech.Modules.GestionIdentite.Domaine;
using MediatR;

namespace CVTech.Modules.GestionIdentite.Application.Features.Authentifier;

/// <summary>Authentifie un utilisateur par e-mail + mot de passe. Action publique.</summary>
public sealed record AuthentifierCommand(string Email, string MotDePasse)
    : IRequest<ResultatAuthentification>;

/// <summary>Identité authentifiée, utilisée pour fabriquer le jeton JWT.</summary>
public sealed record ResultatAuthentification(Guid Id, string Email, RoleUtilisateur Role);
