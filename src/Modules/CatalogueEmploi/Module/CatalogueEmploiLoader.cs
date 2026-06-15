using CVTech.Modules.CatalogueEmploi.Application;
using CVTech.Modules.CatalogueEmploi.Infrastructure;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CVTech.Modules.CatalogueEmploi;

public static class CatalogueEmploiLoader
{
    public static IServiceCollection AddModuleCatalogueEmploi(this IServiceCollection services)
    {
        var assembly = typeof(CatalogueEmploiLoader).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);

        services.AddSingleton<DepotAnnoncesEnMemoire>();
        services.AddSingleton<IDepotAnnonces>(sp => sp.GetRequiredService<DepotAnnoncesEnMemoire>());
        services.AddSingleton<DepotCvEnMemoire>();
        services.AddSingleton<IDepotCv>(sp => sp.GetRequiredService<DepotCvEnMemoire>());
        services.AddSingleton<DepotCandidaturesEnMemoire>();
        services.AddSingleton<IDepotCandidatures>(sp => sp.GetRequiredService<DepotCandidaturesEnMemoire>());

        return services;
    }
}
