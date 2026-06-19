using CVTech.SharedKernel.Domaine;

namespace CVTech.Modules.ActualiteEtAbonnement.Domaine;

/// <summary>Objet de Valeur : origine d'un article agrégé depuis l'extérieur (optionnel).</summary>
public sealed class SourceExterne : ObjetValeur
{
    public string Nom { get; }
    public string Url { get; }

    private SourceExterne(string nom, string url)
    {
        Nom = nom;
        Url = url;
    }

    public static SourceExterne Creer(string nom, string url)
    {
        if (string.IsNullOrWhiteSpace(nom))
            throw new ArgumentException("Le nom de la source est obligatoire.", nameof(nom));
        return new SourceExterne(nom.Trim(), url?.Trim() ?? string.Empty);
    }

    /// <summary>Reconstruit le VO depuis la persistance (sans revalider les invariants).</summary>
    public static SourceExterne Reconstituer(string nom, string url) => new(nom, url);

    protected override IEnumerable<object?> ComposantsEgalite()
    {
        yield return Nom;
        yield return Url;
    }
}
