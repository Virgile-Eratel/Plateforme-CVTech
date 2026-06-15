using CVTech.Modules.AppelOffreFreelance.Application;
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

        return services;
    }
}
