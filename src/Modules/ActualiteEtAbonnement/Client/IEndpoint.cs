using Microsoft.AspNetCore.Routing;

namespace CVTech.Modules.ActualiteEtAbonnement.Client;

/// <summary>
/// Contrat d'un endpoint du module. La couche Client n'expose que cette interface :
/// chaque endpoint concret est défini dans sa vertical slice (Application/Features/**)
/// et se contente de mapper un DTO → Command/Query MediatR.
/// </summary>
public interface IEndpoint
{
    void Map(IEndpointRouteBuilder routes);
}

/// <summary>
/// Endpoint mappé explicitement sur la racine, HORS du groupe « /actualite » (ex. flux RSS
/// public anonyme). Exclu de l'auto-découverte de <c>BrancherEndpoints</c> pour éviter un
/// double mapping sous le groupe.
/// </summary>
public interface IEndpointAutonome : IEndpoint;
