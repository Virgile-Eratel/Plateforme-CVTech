using System.Text.Json;
using Microsoft.JSInterop;

namespace CVTech.Web.Services;

/// <summary>
/// Utilisateur courant côté front, adossé à un jeton JWT (ADR 0008) : l'identité n'est pas
/// envoyée dans les requêtes, seul le jeton l'est (en-tête Bearer).
/// Le jeton est persisté dans le localStorage du navigateur afin de survivre à un rechargement
/// de page (F5) ; il est restauré au démarrage de l'application, sauf s'il est expiré.
/// </summary>
public sealed class SessionUtilisateur(IJSRuntime js)
{
    private const string Cle = "cvtech.session";

    public Guid? UtilisateurId { get; private set; }
    public string? Email { get; private set; }
    public RoleUtilisateur? Role { get; private set; }
    public string? Jeton { get; private set; }

    /// <summary>Vrai une fois que la restauration depuis le localStorage a été tentée.</summary>
    public bool EstRestauree { get; private set; }

    public bool EstConnecte => Jeton is not null;

    /// <summary>Notifie l'UI (layout, badges) d'un changement d'utilisateur courant.</summary>
    public event Action? AuChangement;

    /// <summary>Ouvre une session et la persiste pour qu'elle survive à un rechargement.</summary>
    public async Task OuvrirAsync(Guid id, string email, RoleUtilisateur role, string jeton)
    {
        Affecter(id, email, role, jeton);
        await js.InvokeVoidAsync("localStorage.setItem", Cle,
            JsonSerializer.Serialize(new SessionPersistee(id, email, role, jeton)));
        AuChangement?.Invoke();
    }

    /// <summary>Ferme la session et purge le stockage navigateur.</summary>
    public async Task FermerAsync()
    {
        Affecter(null, null, null, null);
        await js.InvokeVoidAsync("localStorage.removeItem", Cle);
        AuChangement?.Invoke();
    }

    /// <summary>
    /// Restaure la session depuis le localStorage au démarrage. À n'appeler qu'une fois.
    /// Un jeton expiré est ignoré et purgé : l'utilisateur devra se reconnecter.
    /// </summary>
    public async Task RestaurerAsync()
    {
        if (EstRestauree) return;
        EstRestauree = true;

        try
        {
            var brut = await js.InvokeAsync<string?>("localStorage.getItem", Cle);
            if (string.IsNullOrWhiteSpace(brut)) return;

            var session = JsonSerializer.Deserialize<SessionPersistee>(brut);
            if (session is null || JetonExpire(session.Jeton))
            {
                await js.InvokeVoidAsync("localStorage.removeItem", Cle);
                return;
            }

            Affecter(session.Id, session.Email, session.Role, session.Jeton);
            AuChangement?.Invoke();
        }
        catch
        {
            // Stockage illisible/corrompu : on repart d'une session vide, sans bloquer l'app.
        }
    }

    public bool A(RoleUtilisateur role) => Role == role;
    public bool EstAdmin => Role == RoleUtilisateur.Administrateur;

    private void Affecter(Guid? id, string? email, RoleUtilisateur? role, string? jeton)
    {
        UtilisateurId = id;
        Email = email;
        Role = role;
        Jeton = jeton;
    }

    /// <summary>Lit la date d'expiration (« exp ») du JWT pour éviter de restaurer un jeton mort.</summary>
    private static bool JetonExpire(string jeton)
    {
        try
        {
            var parties = jeton.Split('.');
            if (parties.Length < 2) return true;

            using var charge = JsonDocument.Parse(DecoderBase64Url(parties[1]));
            if (!charge.RootElement.TryGetProperty("exp", out var exp)) return false;

            var expiration = DateTimeOffset.FromUnixTimeSeconds(exp.GetInt64());
            return expiration <= DateTimeOffset.UtcNow;
        }
        catch
        {
            return true; // Jeton illisible → considéré comme inutilisable.
        }
    }

    private static byte[] DecoderBase64Url(string valeur)
    {
        var normalise = valeur.Replace('-', '+').Replace('_', '/');
        return Convert.FromBase64String(normalise.PadRight(normalise.Length + (4 - normalise.Length % 4) % 4, '='));
    }

    private sealed record SessionPersistee(Guid Id, string Email, RoleUtilisateur Role, string Jeton);
}
