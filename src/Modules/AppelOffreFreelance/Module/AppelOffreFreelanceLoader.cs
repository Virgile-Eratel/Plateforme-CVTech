using CVTech.Modules.AppelOffreFreelance.Application;
using CVTech.Modules.AppelOffreFreelance.Infrastructure;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CVTech.Modules.AppelOffreFreelance;

public static class AppelOffreFreelanceLoader
{
    public static IServiceCollection AddModuleAppelOffreFreelance(this IServiceCollection services)
    {
        var assembly = typeof(AppelOffreFreelanceLoader).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);

        services.AddSingleton<DepotAppelsOffreEnMemoire>();
        services.AddSingleton<IDepotAppelsOffre>(sp => sp.GetRequiredService<DepotAppelsOffreEnMemoire>());
        services.AddSingleton<DepotPropositionsEnMemoire>();
        services.AddSingleton<IDepotPropositions>(sp => sp.GetRequiredService<DepotPropositionsEnMemoire>());

        return services;
    }
}
