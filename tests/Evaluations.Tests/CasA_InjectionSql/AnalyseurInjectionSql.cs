using System.Text.RegularExpressions;

namespace CVTech.Evaluations.Tests.CasA_InjectionSql;

/// <summary>Une violation potentielle d'injection SQL repérée dans le code source.</summary>
public sealed record ViolationInjectionSql(string Fichier, int Ligne, string Methode, string Raison, string Extrait)
{
    public override string ToString() =>
        $"{Path.GetFileName(Fichier)}:{Ligne} → {Methode}() {Raison}\n        {Extrait}";
}

/// <summary>
/// Cas A — Faille de sécurité masquée (injection SQL).
/// Analyse statique : repère les appels à <c>FromSqlRaw</c> ou à une commande Dapper dont l'argument
/// SQL contient une interpolation (<c>$</c>) ou une concaténation (<c>+</c>) — signes d'une variable
/// d'entrée injectée directement dans la requête au lieu d'être paramétrée.
/// </summary>
public static class AnalyseurInjectionSql
{
    // FromSqlInterpolated est volontairement absent : il paramètre la requête, il est donc SÛR.
    private static readonly string[] MethodesSurveillees =
    [
        "FromSqlRaw", "ExecuteSqlRaw", "ExecuteSqlRawAsync", "SqlQueryRaw",
        // Dapper
        "Query", "QueryAsync", "QueryFirst", "QueryFirstAsync",
        "QueryFirstOrDefault", "QueryFirstOrDefaultAsync",
        "QuerySingle", "QuerySingleAsync", "QuerySingleOrDefault", "QuerySingleOrDefaultAsync",
        "Execute", "ExecuteAsync", "ExecuteScalar", "ExecuteScalarAsync", "ExecuteReader", "ExecuteReaderAsync",
    ];

    private static readonly Regex AppelMethode = new(
        // nom de méthode, éventuels arguments génériques <T>, puis parenthèse ouvrante
        @"\.\s*(?<m>" + string.Join("|", MethodesSurveillees.Select(Regex.Escape)) + @")\s*(<[^>()]*>)?\s*\(",
        RegexOptions.Compiled);

    /// <summary>Analyse un fichier sur disque.</summary>
    public static IReadOnlyList<ViolationInjectionSql> AnalyserFichier(string chemin) =>
        Analyser(File.ReadAllText(chemin), chemin);

    /// <summary>Analyse un contenu source (testable directement avec des extraits en mémoire).</summary>
    public static IReadOnlyList<ViolationInjectionSql> Analyser(string code, string chemin)
    {
        var violations = new List<ViolationInjectionSql>();

        foreach (Match appel in AppelMethode.Matches(code))
        {
            var methode = appel.Groups["m"].Value;
            var debutArgs = appel.Index + appel.Length; // juste après la parenthèse ouvrante
            var argument = ExtraireArgument(code, debutArgs - 1);
            if (argument is null)
                continue;

            var interpolation = argument.Contains('$');
            var concatenation = ContientConcatenation(argument);
            if (!interpolation && !concatenation)
                continue;

            var raison = (interpolation, concatenation) switch
            {
                (true, true) => "contient une interpolation ($) ET une concaténation (+) de variable d'entrée",
                (true, false) => "contient une interpolation de chaîne ($) — variable injectée directement dans le SQL",
                _ => "contient une concaténation (+) — variable d'entrée injectée directement dans le SQL",
            };

            violations.Add(new ViolationInjectionSql(
                chemin,
                NumeroLigne(code, appel.Index),
                methode,
                raison,
                Resumer(methode + "(" + argument + ")")));
        }

        return violations;
    }

    /// <summary>
    /// Extrait le texte de l'argument entre la parenthèse ouvrante (à <paramref name="indexParenthese"/>)
    /// et sa parenthèse fermante équilibrée, en ignorant les parenthèses situées dans des chaînes/commentaires.
    /// </summary>
    private static string? ExtraireArgument(string code, int indexParenthese)
    {
        var profondeur = 0;
        var debut = indexParenthese + 1;
        for (var i = indexParenthese; i < code.Length; i++)
        {
            var c = code[i];

            switch (c)
            {
                case '"' when EstVerbatim(code, i):
                    i = FinChaineVerbatim(code, i);
                    continue;
                case '"':
                    i = FinChaine(code, i);
                    continue;
                case '\'':
                    i = FinCaractere(code, i);
                    continue;
                case '(':
                    profondeur++;
                    break;
                case ')':
                    profondeur--;
                    if (profondeur == 0)
                        return code[debut..i];
                    break;
            }
        }

        return null; // parenthèse non fermée (code incomplet) : on ignore
    }

    /// <summary>Vrai si l'argument concatène une variable à une chaîne (présence d'un littéral et d'un « + »).</summary>
    private static bool ContientConcatenation(string argument)
    {
        if (!argument.Contains('+'))
            return false;

        // On exige la présence d'au moins un littéral de chaîne pour éviter de confondre
        // avec une addition arithmétique sans rapport avec le SQL.
        return argument.Contains('"');
    }

    private static bool EstVerbatim(string code, int i) =>
        (i > 0 && code[i - 1] == '@') ||
        (i > 1 && code[i - 1] == '$' && code[i - 2] == '@') ||
        (i > 1 && code[i - 1] == '@' && code[i - 2] == '$');

    private static int FinChaine(string code, int i)
    {
        for (var j = i + 1; j < code.Length; j++)
        {
            if (code[j] == '\\') { j++; continue; }
            if (code[j] == '"') return j;
        }
        return code.Length - 1;
    }

    private static int FinChaineVerbatim(string code, int i)
    {
        for (var j = i + 1; j < code.Length; j++)
        {
            if (code[j] == '"')
            {
                if (j + 1 < code.Length && code[j + 1] == '"') { j++; continue; } // "" échappé
                return j;
            }
        }
        return code.Length - 1;
    }

    private static int FinCaractere(string code, int i)
    {
        for (var j = i + 1; j < code.Length; j++)
        {
            if (code[j] == '\\') { j++; continue; }
            if (code[j] == '\'') return j;
        }
        return code.Length - 1;
    }

    private static int NumeroLigne(string code, int index) =>
        code.AsSpan(0, index).Count('\n') + 1;

    private static string Resumer(string texte)
    {
        var compact = Regex.Replace(texte, @"\s+", " ").Trim();
        return compact.Length <= 160 ? compact : compact[..157] + "...";
    }
}
