using CVTech.Modules.ActualiteEtAbonnement.Application;
using CVTech.Modules.ActualiteEtAbonnement.Infrastructure;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CVTech.Modules.ActualiteEtAbonnement;

public static class ActualiteEtAbonnementLoader
{
    public static IServiceCollection AddModuleActualiteEtAbonnement(this IServiceCollection services)
    {
        var assembly = typeof(ActualiteEtAbonnementLoader).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);

        // SignalR pour les notifications in-app temps réel (ADR 0009).
        services.AddSignalR();

        services.AddSingleton<DepotArticlesEnMemoire>();
        services.AddSingleton<IDepotArticles>(sp => sp.GetRequiredService<DepotArticlesEnMemoire>());
        services.AddSingleton<DepotAbonnementsEnMemoire>();
        services.AddSingleton<IDepotAbonnements>(sp => sp.GetRequiredService<DepotAbonnementsEnMemoire>());
        services.AddSingleton<DepotNotificationsEnMemoire>();
        services.AddSingleton<IDepotNotifications>(sp => sp.GetRequiredService<DepotNotificationsEnMemoire>());

        services.AddSingleton<IGenerateurRss, GenerateurRss>();
        services.AddScoped<INotificateurTempsReel, NotificateurSignalR>();

        return services;
    }
}
