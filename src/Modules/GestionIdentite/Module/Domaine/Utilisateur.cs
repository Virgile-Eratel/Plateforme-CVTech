using CVTech.SharedKernel.Domaine;

namespace CVTech.Modules.GestionIdentite.Domaine;

/// <summary>
/// Agrégat représentant un compte utilisateur (Candidat, Entreprise ou Administrateur).
/// Porte l'invariant de blocage : un compte bloqué ne peut exécuter aucune action authentifiée.
/// </summary>
public sealed class Utilisateur : RacineAgregat<Guid>
{
    public string Email { get; private set; } = default!;
    public RoleUtilisateur Role { get; private set; }
    public bool EstBloque { get; private set; }

    private Utilisateur() { } // pour la reconstitution par l'infrastructure

    public static Utilisateur Inscrire(string email, RoleUtilisateur role)
    {
        if (!EstEmailValide(email))
            throw new EmailInvalideException(email ?? string.Empty);

        return new Utilisateur
        {
            Id = Guid.NewGuid(),
            Email = email.Trim(),
            Role = role,
            EstBloque = false
        };
    }

    public void Bloquer()
    {
        if (Role == RoleUtilisateur.Administrateur)
            throw new InvalidOperationException("Un administrateur ne peut pas être bloqué.");
        EstBloque = true;
    }

    public void Reactiver() => EstBloque = false;

    private static bool EstEmailValide(string? email) =>
        !string.IsNullOrWhiteSpace(email)
        && email.Contains('@')
        && email.IndexOf('@') > 0
        && email.IndexOf('@') < email.Length - 1;
}
