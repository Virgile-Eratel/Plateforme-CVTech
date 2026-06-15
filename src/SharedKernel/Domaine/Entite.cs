namespace CVTech.SharedKernel.Domaine;

/// <summary>Base d'une entité : identité par <typeparamref name="TId"/>.</summary>
public abstract class Entite<TId> where TId : notnull
{
    public TId Id { get; protected set; } = default!;

    public override bool Equals(object? obj) =>
        obj is Entite<TId> autre
        && autre.GetType() == GetType()
        && EqualityComparer<TId>.Default.Equals(Id, autre.Id);

    public override int GetHashCode() => EqualityComparer<TId>.Default.GetHashCode(Id);
}
