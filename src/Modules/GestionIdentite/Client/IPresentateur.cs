using Microsoft.AspNetCore.Http;

namespace CVTech.Modules.GestionIdentite.Client;

/// <summary>
/// Port de SORTIE de l'adaptateur HTTP : traduit une exception métier remontée par un
/// cas d'usage en réponse HTTP. Appliqué de façon transverse par le wiring (filtre
/// d'endpoint), de sorte qu'aucun endpoint ne porte de try/catch HTTP. Retourne
/// <c>null</c> si l'exception n'est pas gérée (elle est alors propagée normalement).
/// </summary>
public interface IPresentateur
{
    IResult? PresenterException(Exception exception);
}
