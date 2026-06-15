using CVTech.SharedKernel.Evenements;

namespace CVTech.SharedKernel.Domaine;

/// <summary>Racine d'agrégat : porte les événements d'intégration à publier.</summary>
public abstract class RacineAgregat<TId> : Entite<TId> where TId : notnull
{
    private readonly List<IEvenementIntegration> _evenements = [];

    public IReadOnlyCollection<IEvenementIntegration> EvenementsNonPublies => _evenements.AsReadOnly();

    protected void EnregistrerEvenement(IEvenementIntegration evenement) => _evenements.Add(evenement);

    public void ViderEvenements() => _evenements.Clear();
}
