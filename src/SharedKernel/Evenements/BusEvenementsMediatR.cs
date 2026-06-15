using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CVTech.SharedKernel.Evenements;

/// <summary>
/// Implémentation du bus interne reposant sur les notifications MediatR : la
/// publication est dispatchée à tous les <c>INotificationHandler</c> enregistrés,
/// quel que soit le module qui les héberge.
/// </summary>
public sealed class BusEvenementsMediatR(IPublisher publisher) : IBusEvenements
{
    public Task PublierAsync(IEvenementIntegration evenement, CancellationToken ct = default) =>
        publisher.Publish(evenement, ct);
}

public static class BusEvenementsExtensions
{
    public static IServiceCollection AddBusEvenements(this IServiceCollection services)
    {
        services.AddScoped<IBusEvenements, BusEvenementsMediatR>();
        return services;
    }
}
