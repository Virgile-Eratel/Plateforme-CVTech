using CVTech.SharedKernel.Domaine;

namespace CVTech.Modules.AppelOffreFreelance.Domaine;

/// <summary>Objet de Valeur : Taux Journalier Moyen d'une proposition (toujours positif).</summary>
public sealed class BaremeTJM : ObjetValeur
{
    public decimal MontantJournalier { get; }

    private BaremeTJM(decimal montant) => MontantJournalier = montant;

    public static BaremeTJM Creer(decimal montant)
    {
        if (montant <= 0)
            throw new ArgumentException("Le TJM doit être strictement positif.", nameof(montant));
        return new BaremeTJM(montant);
    }

    protected override IEnumerable<object?> ComposantsEgalite()
    {
        yield return MontantJournalier;
    }
}
