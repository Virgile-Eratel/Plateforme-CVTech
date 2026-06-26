namespace CVTech.Evaluations.Tests.CasA_InjectionSql;

/// <summary>
/// Retrouve la racine du dépôt puis énumère les fichiers C# de production à inspecter.
/// L'analyse EDD porte sur le code *généré* (src/Modules/**), pas sur le code de test ni le code généré par EF.
/// </summary>
public static class LocalisateurSources
{
    /// <summary>Remonte l'arborescence jusqu'au dossier contenant la solution (marqueur stable du dépôt).</summary>
    public static string RacineDepot()
    {
        var repertoire = new DirectoryInfo(AppContext.BaseDirectory);
        while (repertoire is not null)
        {
            if (repertoire.EnumerateFiles("CVTech.slnx").Any())
                return repertoire.FullName;
            repertoire = repertoire.Parent;
        }

        throw new DirectoryNotFoundException(
            "Impossible de localiser la racine du dépôt (fichier CVTech.slnx introuvable en remontant depuis "
            + AppContext.BaseDirectory + ").");
    }

    /// <summary>Tous les fichiers .cs de production des modules, hors artefacts de build et code EF généré.</summary>
    public static IReadOnlyList<string> FichiersDeProduction()
    {
        var modules = Path.Combine(RacineDepot(), "src", "Modules");

        return Directory
            .EnumerateFiles(modules, "*.cs", SearchOption.AllDirectories)
            .Where(chemin => !EstArtefact(chemin))
            .ToList();
    }

    private static bool EstArtefact(string chemin)
    {
        var segments = chemin.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        // bin/ et obj/ : sorties de compilation. Migrations/ : SQL généré automatiquement par EF Core,
        // qui n'est pas du code écrit par l'agent et peut contenir des chaînes SQL légitimes.
        return segments.Any(s =>
            s.Equals("bin", StringComparison.OrdinalIgnoreCase) ||
            s.Equals("obj", StringComparison.OrdinalIgnoreCase) ||
            s.Equals("Migrations", StringComparison.OrdinalIgnoreCase));
    }
}
