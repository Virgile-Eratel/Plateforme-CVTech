using CVTech.Modules.ActualiteEtAbonnement.Application;
using CVTech.Modules.ActualiteEtAbonnement.Infrastructure;
using CVTech.Modules.ActualiteEtAbonnement.Infrastructure.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CVTech.Modules.ActualiteEtAbonnement;

public static class ActualiteEtAbonnementLoader
{
    public static IServiceCollection AddModuleActualiteEtAbonnement(
        this IServiceCollection services, Action<DbContextOptionsBuilder> configurerBdd)
    {
        var assembly = typeof(ActualiteEtAbonnementLoader).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);

        // SignalR pour les notifications in-app temps réel (ADR 0009).
        services.AddSignalR();

        // Persistance EF Core / Azure SQL (schéma « actualite »).
        services.AddDbContext<ActualiteDbContext>(configurerBdd);
        services.AddScoped<IDepotArticles, DepotArticlesEfCore>();
        services.AddScoped<IDepotAbonnements, DepotAbonnementsEfCore>();
        services.AddScoped<IDepotNotifications, DepotNotificationsEfCore>();

        services.AddSingleton<IGenerateurRss, GenerateurRss>();
        services.AddScoped<INotificateurTempsReel, NotificateurSignalR>();

        return services;
    }
}
