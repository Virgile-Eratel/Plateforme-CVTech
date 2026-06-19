using CVTech.Modules.CatalogueEmploi.Application;
using CVTech.Modules.CatalogueEmploi.Infrastructure;
using CVTech.Modules.CatalogueEmploi.Infrastructure.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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
        services.AddScoped<IDepotAnnonces, DepotAnnoncesEfCore>();
        services.AddScoped<IDepotCv, DepotCvEfCore>();
        services.AddScoped<IDepotCandidatures, DepotCandidaturesEfCore>();

        return services;
    }
}
