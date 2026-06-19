using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace CVTech.Modules.GestionIdentite.Client;

/// <summary>
/// Branche le port de sortie <see cref="IPresentateur"/> sur un groupe de routes : les
/// exceptions métier remontées par un endpoint sont traduites en réponses HTTP ; les
/// autres exceptions se propagent normalement. Permet aux endpoints de rester de purs
/// mappeurs DTO ↔ Command/Query, sans plomberie HTTP.
/// </summary>
internal static class FiltrePresentateur
{
    public static RouteGroupBuilder AppliquerPresentateur(this RouteGroupBuilder groupe) =>
        groupe.AddEndpointFilter(async (contexte, suite) =>
        {
            try { return await suite(contexte); }
            catch (Exception ex) when (
                contexte.HttpContext.RequestServices.GetService<IPresentateur>() is { } presentateur &&
                presentateur.PresenterException(ex) is { } reponse)
            {
                return reponse;
            }
        });

    /// <summary>
    /// Instancie et branche chaque <see cref="IEndpoint"/> défini dans l'assembly de CE
    /// module (les endpoints sont des mappeurs sans état, leurs dépendances arrivant par
    /// les paramètres du handler de route). Garantit un scoping par module.
    /// </summary>
    public static RouteGroupBuilder BrancherEndpoints(this RouteGroupBuilder groupe)
    {
        foreach (var type in typeof(FiltrePresentateur).Assembly.GetTypes()
                     .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IEndpoint).IsAssignableFrom(t)))
            ((IEndpoint)Activator.CreateInstance(type)!).Map(groupe);

        return groupe;
    }
}
