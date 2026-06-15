namespace CVTech.SharedKernel.Domaine;

/// <summary>
/// Objet de Valeur partagé identifiant un champ d'expertise
/// (ex : « Cloud Azure » → code « cloud-azure »). Utilisé par les modules
/// CatalogueEmploi, AppelOffreFreelance et ActualiteEtAbonnement.
/// </summary>
public sealed class DomaineMetier : ObjetValeur
{
    public string Code { get; }
    public string Libelle { get; }

    private DomaineMetier(string code, string libelle)
    {
        Code = code;
        Libelle = libelle;
    }

    public static DomaineMetier Creer(string libelle)
    {
        if (string.IsNullOrWhiteSpace(libelle))
            throw new ArgumentException("Le domaine métier ne peut pas être vide.", nameof(libelle));

        return new DomaineMetier(Slugifier(libelle), libelle.Trim());
    }

    /// <summary>Reconstruit le VO depuis la persistance (code déjà calculé).</summary>
    public static DomaineMetier Reconstituer(string code, string libelle) => new(code, libelle);

    private static string Slugifier(string valeur)
    {
        var minuscule = valeur.Trim().ToLowerInvariant();
        var caracteres = minuscule.Select(c => char.IsLetterOrDigit(c) ? c : '-');
        var slug = new string(caracteres.ToArray());
        while (slug.Contains("--")) slug = slug.Replace("--", "-");
        return slug.Trim('-');
    }

    protected override IEnumerable<object?> ComposantsEgalite()
    {
        yield return Code;
    }

    public override string ToString() => Libelle;
}
