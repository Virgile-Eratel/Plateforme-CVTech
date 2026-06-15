namespace CVTech.SharedKernel.Domaine;

/// <summary>Base d'un Objet de Valeur : égalité structurelle par composants.</summary>
public abstract class ObjetValeur
{
    protected abstract IEnumerable<object?> ComposantsEgalite();

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType()) return false;
        var autre = (ObjetValeur)obj;
        return ComposantsEgalite().SequenceEqual(autre.ComposantsEgalite());
    }

    public override int GetHashCode() =>
        ComposantsEgalite().Aggregate(0, (acc, c) => HashCode.Combine(acc, c));
}
