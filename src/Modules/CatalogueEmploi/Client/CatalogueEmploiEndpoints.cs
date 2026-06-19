using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CVTech.Modules.CatalogueEmploi.Client;

/// <summary>
/// Seule porte d'entrée du module : crée le groupe de routes « /emploi », branche le
/// port de sortie, puis délègue le mapping à chaque <see cref="IEndpoint"/> du module
/// (défini dans Application/Features/**). Aucune route en dur ici — uniquement le branchement.
/// </summary>
public static class CatalogueEmploiEndpoints
{
    public static IEndpointRouteBuilder MapCatalogueEmploi(this IEndpointRouteBuilder routes)
    {
        var groupe = routes.MapGroup("/emploi").WithTags("CatalogueEmploi");

        // Port de sortie : traduit les exceptions métier en réponses HTTP, pour tout le groupe.
        groupe.AppliquerPresentateur();

        // Découverte des endpoints du SEUL assembly de ce module (scoping par module).
        groupe.BrancherEndpoints();

        return routes;
    }
}
