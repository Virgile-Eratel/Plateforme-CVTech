namespace CVTech.Web.Services;

/// <summary>
/// « Utilisateur courant » côté front. Tant que l'authentification JWT n'est pas en place
/// (ADR 0008), l'identité est portée par le client et injectée dans les requêtes — d'où
/// l'importance de finaliser l'auth avant déploiement. Un seul utilisateur actif à la fois.
/// </summary>
public sealed class SessionUtilisateur
{
    public Guid? UtilisateurId { get; private set; }
    public string? Email { get; private set; }
    public RoleUtilisateur? Role { get; private set; }

    public bool EstConnecte => UtilisateurId is not null;

    /// <summary>Notifie l'UI (layout, badges) d'un changement d'utilisateur courant.</summary>
    public event Action? AuChangement;

    public void Connecter(Guid id, string email, RoleUtilisateur role)
    {
        UtilisateurId = id;
        Email = email;
        Role = role;
        AuChangement?.Invoke();
    }

    public void Deconnecter()
    {
        UtilisateurId = null;
        Email = null;
        Role = null;
        AuChangement?.Invoke();
    }

    public bool A(RoleUtilisateur role) => Role == role;
    public bool EstAdmin => Role == RoleUtilisateur.Administrateur;
}
