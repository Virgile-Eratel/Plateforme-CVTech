using CVTech.Modules.GestionIdentite.Domaine;

namespace CVTech.Modules.GestionIdentite.Infrastructure.Persistence.Entities;

/// <summary>
/// Entité de persistance EF Core (DAO plat) de l'agrégat <see cref="Utilisateur"/>.
/// Aucune logique métier : simples propriétés mutables mappées en colonnes.
/// </summary>
public sealed class UtilisateurEntity
{
    public Guid Id { get; set; }
    public string Email { get; set; } = default!;
    public RoleUtilisateur Role { get; set; }
    public bool EstBloque { get; set; }
    public string? MotDePasseHash { get; set; }
}
