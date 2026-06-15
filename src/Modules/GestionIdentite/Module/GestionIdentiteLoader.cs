using CVTech.Modules.GestionIdentite.Application;
using CVTech.Modules.GestionIdentite.Contracts;
using CVTech.Modules.GestionIdentite.Infrastructure;
using CVTech.Modules.GestionIdentite.Infrastructure.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CVTech.Modules.GestionIdentite;

/// <summary>
/// Composition Root du module : enregistre MediatR (handlers de ce module),
/// les validateurs, l'infrastructure EF Core et le contrat public IVerificateurPermission.
/// </summary>
public static class GestionIdentiteLoader
{
    public static IServiceCollection AddModuleGestionIdentite(
        this IServiceCollection services, Action<DbContextOptionsBuilder> configurerBdd)
    {
        var assembly = typeof(GestionIdentiteLoader).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);

        // Persistance EF Core / Azure SQL (schéma « identite »).
        services.AddDbContext<IdentiteDbContext>(configurerBdd);
        services.AddScoped<IDepotUtilisateurs, DepotUtilisateursEfCore>();

        // Contrat public consommé par les autres modules.
        services.AddScoped<IVerificateurPermission, VerificateurPermission>();

        return services;
    }
}
