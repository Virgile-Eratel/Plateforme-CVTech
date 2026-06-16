using System.Security.Claims;

namespace CVTech.SharedKernel.Securite;

/// <summary>
/// Extraction de l'identité authentifiée depuis les claims du jeton JWT.
/// L'identité provient TOUJOURS du jeton (ADR 0008) — jamais d'un champ de DTO,
/// ce qui supprime le risque d'usurpation.
/// </summary>
public static class IdentiteClaims
{
    /// <summary>Identifiant de l'appelant (claim « sub » / NameIdentifier).</summary>
    public static Guid IdUtilisateur(this ClaimsPrincipal principal)
    {
        var valeur = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? principal.FindFirst("sub")?.Value;
        return Guid.TryParse(valeur, out var id)
            ? id
            : throw new InvalidOperationException("Jeton sans identifiant utilisateur valide.");
    }
}
