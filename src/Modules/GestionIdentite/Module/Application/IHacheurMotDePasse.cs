namespace CVTech.Modules.GestionIdentite.Application;

/// <summary>
/// Port de hachage des mots de passe (implémenté dans l'Infrastructure via ASP.NET Core Identity).
/// Le Domaine et l'Application ne connaissent jamais l'algorithme.
/// </summary>
public interface IHacheurMotDePasse
{
    string Hacher(string motDePasseClair);
    bool Verifier(string hash, string motDePasseClair);
}
