using CVTech.Modules.GestionIdentite.Domaine;
using CVTech.Modules.GestionIdentite.Infrastructure.Persistence.Entities;

namespace CVTech.Modules.GestionIdentite.Infrastructure.Persistence.Mappers;

/// <summary>Conversions entre l'agrégat <see cref="Utilisateur"/> et l'entité EF <see cref="UtilisateurEntity"/>.</summary>
public static class UtilisateurMapper
{
    public static UtilisateurEntity ToEntity(Utilisateur domaine) => new()
    {
        Id = domaine.Id,
        Email = domaine.Email,
        Role = domaine.Role,
        EstBloque = domaine.EstBloque,
        MotDePasseHash = domaine.MotDePasseHash
    };

    public static Utilisateur ToDomain(UtilisateurEntity entite) =>
        Utilisateur.Reconstituer(entite.Id, entite.Email, entite.Role, entite.EstBloque, entite.MotDePasseHash);

    /// <summary>Reporte l'état mutable de l'agrégat sur une entité déjà suivie (mise à jour).</summary>
    public static void Appliquer(Utilisateur domaine, UtilisateurEntity entite)
    {
        entite.Email = domaine.Email;
        entite.Role = domaine.Role;
        entite.EstBloque = domaine.EstBloque;
        entite.MotDePasseHash = domaine.MotDePasseHash;
    }
}
