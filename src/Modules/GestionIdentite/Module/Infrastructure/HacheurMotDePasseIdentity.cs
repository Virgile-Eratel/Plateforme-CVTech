using CVTech.Modules.GestionIdentite.Application;
using Microsoft.AspNetCore.Identity;

namespace CVTech.Modules.GestionIdentite.Infrastructure;

/// <summary>
/// Implémentation du port <see cref="IHacheurMotDePasse"/> via le hacheur d'ASP.NET Core Identity
/// (PBKDF2 salé, ADR 0008). Aucun mot de passe n'est jamais stocké en clair.
/// </summary>
public sealed class HacheurMotDePasseIdentity : IHacheurMotDePasse
{
    private readonly PasswordHasher<object> _hacheur = new();
    private static readonly object Porteur = new();

    public string Hacher(string motDePasseClair) => _hacheur.HashPassword(Porteur, motDePasseClair);

    public bool Verifier(string hash, string motDePasseClair) =>
        _hacheur.VerifyHashedPassword(Porteur, hash, motDePasseClair) != PasswordVerificationResult.Failed;
}
