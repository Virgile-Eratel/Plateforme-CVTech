using CVTech.Modules.GestionIdentite.Application.Features.Authentifier;
using CVTech.Modules.GestionIdentite.Domaine;
using CVTech.SharedKernel.Permissions;
using Microsoft.AspNetCore.Http;

namespace CVTech.Modules.GestionIdentite.Client;

/// <summary>
/// Implémentation par défaut du port de sortie : mappe les exceptions métier du module
/// vers des statuts HTTP. Aucune logique métier — uniquement la traduction de présentation.
/// </summary>
internal sealed class PresentateurHttp : IPresentateur
{
    public IResult? PresenterException(Exception exception) => exception switch
    {
        PermissionRefuseeException ex =>
            Results.Problem(ex.Message, statusCode: StatusCodes.Status403Forbidden),
        AuthentificationException ex =>
            Results.Problem(ex.Message, statusCode: StatusCodes.Status401Unauthorized),
        RoleInscriptionInterditException ex =>
            Results.Problem(ex.Message, statusCode: StatusCodes.Status403Forbidden),
        _ => null
    };
}
