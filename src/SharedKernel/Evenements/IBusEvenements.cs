namespace CVTech.SharedKernel.Evenements;

/// <summary>
/// Bus d'événements interne en mémoire (ADR 0003). Un module publie un événement
/// sans connaître ses consommateurs ; les modules abonnés réagissent via un handler.
/// </summary>
public interface IBusEvenements
{
    Task PublierAsync(IEvenementIntegration evenement, CancellationToken ct = default);
}
