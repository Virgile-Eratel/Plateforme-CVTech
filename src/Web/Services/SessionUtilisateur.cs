namespace CVTech.Web.Services;

/// <summary>
/// Utilisateur courant côté front, désormais adossé à un jeton JWT (ADR 0008) :
/// l'identité n'est plus envoyée dans les requêtes, seul le jeton l'est (en-tête Bearer).
/// </summary>
public sealed class SessionUtilisateur
{
    public Guid? UtilisateurId { get; private set; }
    public string? Email { get; private set; }
    public RoleUtilisateur? Role { get; private set; }
    public string? Jeton { get; private set; }

    public bool EstConnecte => Jeton is not null;

    /// <summary>Notifie l'UI (layout, badges) d'un changement d'utilisateur courant.</summary>
    public event Action? AuChangement;

    public void Ouvrir(Guid id, string email, RoleUtilisateur role, string jeton)
    {
        UtilisateurId = id;
        Email = email;
        Role = role;
        Jeton = jeton;
        AuChangement?.Invoke();
    }

    public void Fermer()
    {
        UtilisateurId = null;
        Email = null;
        Role = null;
        Jeton = null;
        AuChangement?.Invoke();
    }

    public bool A(RoleUtilisateur role) => Role == role;
    public bool EstAdmin => Role == RoleUtilisateur.Administrateur;
}
