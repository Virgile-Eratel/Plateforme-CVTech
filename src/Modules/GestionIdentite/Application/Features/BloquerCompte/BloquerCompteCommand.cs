using MediatR;

namespace CVTech.Modules.GestionIdentite.Application.Features.BloquerCompte;

/// <summary>
/// Bloque un compte. Action réservée à l'Administrateur (matrice de permissions).
/// </summary>
public sealed record BloquerCompteCommand(Guid AppelantId, Guid CompteCibleId) : IRequest;
