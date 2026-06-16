using CVTech.SharedKernel.Permissions;
using Microsoft.AspNetCore.Http;

namespace CVTech.Modules.AppelOffreFreelance.Application;

/// <summary>
/// Plomberie HTTP partagée par les endpoints des vertical slices : traduit les exceptions
/// métier d'autorisation en réponses HTTP. Ne contient aucune logique métier.
/// </summary>
public static class EndpointHttp
{
    public static async Task<IResult> Executer(Func<Task<IResult>> action)
    {
        try { return await action(); }
        catch (PermissionRefuseeException ex)
        {
            return Results.Problem(ex.Message, statusCode: StatusCodes.Status403Forbidden);
        }
    }
}
