using CVTech.SharedKernel.Domaine;

namespace CVTech.Modules.AppelOffreFreelance.Domaine;

/// <summary>Objet de Valeur : Taux Journalier Moyen d'une proposition (toujours positif).</summary>
public sealed class BaremeTJM : ObjetValeur
{
    public decimal MontantJournalier { get; }

    // Nom du paramètre aligné sur la propriété pour permettre le binding du constructeur par EF Core.
    private BaremeTJM(decimal montantJournalier) => MontantJournalier = montantJournalier;

    public static BaremeTJM Creer(decimal montant)
    {
        if (montant <= 0)
            throw new ArgumentException("Le TJM doit être strictement positif.", nameof(montant));
        return new BaremeTJM(montant);
    }

    /// <summary>Reconstruit le VO depuis la persistance, sans revalider les invariants.</summary>
    public static BaremeTJM Reconstituer(decimal montantJournalier) => new(montantJournalier);

    protected override IEnumerable<object?> ComposantsEgalite()
    {
        yield return MontantJournalier;
    }
}
