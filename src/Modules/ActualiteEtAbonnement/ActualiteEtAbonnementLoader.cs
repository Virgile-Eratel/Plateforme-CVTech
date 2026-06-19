using CVTech.Modules.ActualiteEtAbonnement.Application;
using CVTech.Modules.ActualiteEtAbonnement.Client;
using CVTech.Modules.ActualiteEtAbonnement.Infrastructure;
using CVTech.Modules.ActualiteEtAbonnement.Infrastructure.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using CVTech.Modules.ActualiteEtAbonnement.Domaine;

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
        services.AddScoped<IDepotArticles, ArticleRepository>();
        services.AddScoped<IDepotAbonnements, AbonnementRepository>();
        services.AddScoped<IDepotNotifications, NotificationRepository>();

        services.AddSingleton<IGenerateurRss, GenerateurRss>();
        services.AddScoped<INotificateurTempsReel, NotificateurSignalR>();

        // Port de sortie de l'adaptateur HTTP (traduction exception métier → réponse).
        // Les endpoints (IEndpoint) sont, eux, découverts par le wiring (Client), pas via DI.
        services.AddSingleton<IPresentateur, PresentateurHttp>();

        return services;
    }
}
