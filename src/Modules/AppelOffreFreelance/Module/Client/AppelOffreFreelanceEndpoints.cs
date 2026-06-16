using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace CVTech.Modules.AppelOffreFreelance.Client;

/// <summary>
/// Seule porte d'entrée du module : crée le groupe de routes « /freelance » puis délègue
/// le mapping à chaque <see cref="IEndpoint"/> (défini dans Application/Features/**).
/// Aucune route ni logique d'appel ici — uniquement le branchement.
/// </summary>
public static class AppelOffreFreelanceEndpoints
{
    public static IEndpointRouteBuilder MapAppelOffreFreelance(this IEndpointRouteBuilder routes)
    {
        var groupe = routes.MapGroup("/freelance").WithTags("AppelOffreFreelance");

        foreach (var endpoint in routes.ServiceProvider.GetServices<IEndpoint>())
            endpoint.Map(groupe);

        return routes;
    }
}
