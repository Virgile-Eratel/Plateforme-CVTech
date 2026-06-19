using CVTech.Modules.AppelOffreFreelance.Application;
using CVTech.Modules.AppelOffreFreelance.Client;
using CVTech.Modules.AppelOffreFreelance.Infrastructure;
using CVTech.Modules.AppelOffreFreelance.Infrastructure.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using CVTech.Modules.AppelOffreFreelance.Domaine;

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
        services.AddScoped<IDepotAppelsOffre, AppelOffreRepository>();
        services.AddScoped<IDepotPropositions, PropositionRepository>();

        // Port de sortie de l'adaptateur HTTP (traduction exception métier → réponse).
        // Les endpoints (IEndpoint) sont, eux, découverts par le wiring (Client), pas via DI.
        services.AddSingleton<IPresentateur, PresentateurHttp>();

        return services;
    }
}
