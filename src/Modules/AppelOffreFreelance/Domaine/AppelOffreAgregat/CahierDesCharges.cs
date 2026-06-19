using CVTech.SharedKernel.Domaine;

namespace CVTech.Modules.AppelOffreFreelance.Domaine;

/// <summary>Objet de Valeur : description d'un appel d'offre.</summary>
public sealed class CahierDesCharges : ObjetValeur
{
    public string Contexte { get; }
    public string Livrables { get; }
    public DateTimeOffset Deadline { get; }
    public decimal BudgetMin { get; }
    public decimal BudgetMax { get; }

    private CahierDesCharges(
        string contexte, string livrables, DateTimeOffset deadline, decimal budgetMin, decimal budgetMax)
    {
        Contexte = contexte;
        Livrables = livrables;
        Deadline = deadline;
        BudgetMin = budgetMin;
        BudgetMax = budgetMax;
    }

    public static CahierDesCharges Creer(
        string contexte, string livrables, DateTimeOffset deadline, decimal budgetMin, decimal budgetMax)
    {
        if (string.IsNullOrWhiteSpace(contexte))
            throw new ArgumentException("Le contexte est obligatoire.", nameof(contexte));
        if (string.IsNullOrWhiteSpace(livrables))
            throw new ArgumentException("Les livrables attendus sont obligatoires.", nameof(livrables));
        if (budgetMin < 0 || budgetMax < budgetMin)
            throw new ArgumentException("La fourchette budgétaire est invalide.");

        return new CahierDesCharges(contexte.Trim(), livrables.Trim(), deadline, budgetMin, budgetMax);
    }

    /// <summary>Reconstruit le VO depuis la persistance, sans revalider les invariants.</summary>
    public static CahierDesCharges Reconstituer(
        string contexte, string livrables, DateTimeOffset deadline, decimal budgetMin, decimal budgetMax) =>
        new(contexte, livrables, deadline, budgetMin, budgetMax);

    protected override IEnumerable<object?> ComposantsEgalite()
    {
        yield return Contexte;
        yield return Livrables;
        yield return Deadline;
        yield return BudgetMin;
        yield return BudgetMax;
    }
}
