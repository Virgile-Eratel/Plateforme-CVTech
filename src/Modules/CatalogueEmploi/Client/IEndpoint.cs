using Microsoft.AspNetCore.Routing;

namespace CVTech.Modules.CatalogueEmploi.Client;

/// <summary>
/// Contrat d'un endpoint du module. La couche Client n'expose que cette interface :
/// chaque endpoint concret est défini dans sa vertical slice (Application/Features/**)
/// et se contente de mapper un DTO → Command/Query MediatR.
/// </summary>
public interface IEndpoint
{
    void Map(IEndpointRouteBuilder routes);
}
