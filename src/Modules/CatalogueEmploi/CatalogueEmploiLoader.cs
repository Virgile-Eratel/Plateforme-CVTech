using CVTech.Modules.CatalogueEmploi.Application;
using CVTech.Modules.CatalogueEmploi.Client;
using CVTech.Modules.CatalogueEmploi.Infrastructure;
using CVTech.Modules.CatalogueEmploi.Infrastructure.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using CVTech.Modules.CatalogueEmploi.Domaine;

namespace CVTech.Modules.CatalogueEmploi;

public static class CatalogueEmploiLoader
{
    public static IServiceCollection AddModuleCatalogueEmploi(
        this IServiceCollection services, Action<DbContextOptionsBuilder> configurerBdd)
    {
        var assembly = typeof(CatalogueEmploiLoader).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);

        // Persistance EF Core / Azure SQL (schéma « emploi »).
        services.AddDbContext<EmploiDbContext>(configurerBdd);
        services.AddScoped<IDepotAnnonces, AnnonceRepository>();
        services.AddScoped<IDepotCv, CvRepository>();
        services.AddScoped<IDepotCandidatures, CandidatureRepository>();

        // Port de sortie de l'adaptateur HTTP (traduction exception métier → réponse).
        // Les endpoints (IEndpoint) sont, eux, découverts par le wiring (Client), pas via DI.
        services.AddSingleton<IPresentateur, PresentateurHttp>();

        return services;
    }
}
