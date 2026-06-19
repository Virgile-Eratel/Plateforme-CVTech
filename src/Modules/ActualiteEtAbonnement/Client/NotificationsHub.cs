using CVTech.SharedKernel.Securite;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CVTech.Modules.ActualiteEtAbonnement.Client;

/// <summary>
/// Hub SignalR des notifications in-app. Authentifié : chaque connexion rejoint
/// automatiquement le groupe portant l'identifiant de l'utilisateur du jeton — un client
/// ne peut donc plus écouter les notifications d'autrui.
/// </summary>
[Authorize]
public sealed class NotificationsHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var utilisateurId = Context.User!.IdUtilisateur();
        await Groups.AddToGroupAsync(Context.ConnectionId, utilisateurId.ToString());
        await base.OnConnectedAsync();
    }
}
