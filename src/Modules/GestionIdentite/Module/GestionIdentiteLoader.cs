using CVTech.Modules.GestionIdentite.Application;
using CVTech.Modules.GestionIdentite.Contracts;
using CVTech.Modules.GestionIdentite.Infrastructure;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CVTech.Modules.GestionIdentite;

/// <summary>
/// Composition Root du module : enregistre MediatR (handlers de ce module),
/// les validateurs, l'infrastructure et le contrat public IVerificateurPermission.
/// </summary>
public static class GestionIdentiteLoader
{
    public static IServiceCollection AddModuleGestionIdentite(this IServiceCollection services)
    {
        var assembly = typeof(GestionIdentiteLoader).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);

        // Persistance (pilote : en mémoire ; EF Core / Azure SQL en Phase 3 bis).
        services.AddSingleton<DepotUtilisateursEnMemoire>();
        services.AddSingleton<IDepotUtilisateurs>(sp => sp.GetRequiredService<DepotUtilisateursEnMemoire>());

        // Contrat public consommé par les autres modules.
        services.AddScoped<IVerificateurPermission, VerificateurPermission>();

        return services;
    }
}
