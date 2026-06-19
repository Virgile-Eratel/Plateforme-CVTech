using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CVTech.Modules.AppelOffreFreelance.Client;

/// <summary>
/// Seule porte d'entrée du module : crée le groupe de routes « /freelance », branche le
/// port de sortie, puis délègue le mapping à chaque <see cref="IEndpoint"/> du module
/// (défini dans Application/Features/**). Aucune route en dur ici — uniquement le branchement.
/// </summary>
public static class AppelOffreFreelanceEndpoints
{
    public static IEndpointRouteBuilder MapAppelOffreFreelance(this IEndpointRouteBuilder routes)
    {
        var groupe = routes.MapGroup("/freelance").WithTags("AppelOffreFreelance");

        // Port de sortie : traduit les exceptions métier en réponses HTTP, pour tout le groupe.
        groupe.AppliquerPresentateur();

        // Découverte des endpoints du SEUL assembly de ce module (scoping par module).
        groupe.BrancherEndpoints();

        return routes;
    }
}
