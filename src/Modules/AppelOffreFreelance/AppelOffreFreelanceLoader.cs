using CVTech.Modules.AppelOffreFreelance.Application;
using CVTech.Modules.AppelOffreFreelance.Client;
using CVTech.Modules.AppelOffreFreelance.Infrastructure;
using CVTech.Modules.AppelOffreFreelance.Infrastructure.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CVTech.Modules.AppelOffreFreelance;

public static class AppelOffreFreelanceLoader
{
    public static IServiceCollection AddModuleAppelOffreFreelance(
        this IServiceCollection services, Action<DbContextOptionsBuilder> configurerBdd)
    {
        var assembly = typeof(AppelOffreFreelanceLoader).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);

        // Persistance EF Core / Azure SQL (schéma « freelance »).
        services.AddDbContext<FreelanceDbContext>(configurerBdd);
        services.AddScoped<IDepotAppelsOffre, DepotAppelsOffreEfCore>();
        services.AddScoped<IDepotPropositions, DepotPropositionsEfCore>();

        // Endpoints des vertical slices (un IEndpoint par feature, défini dans Application).
        foreach (var type in assembly.GetTypes()
                     .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IEndpoint).IsAssignableFrom(t)))
            services.AddSingleton(typeof(IEndpoint), type);

        return services;
    }
}
