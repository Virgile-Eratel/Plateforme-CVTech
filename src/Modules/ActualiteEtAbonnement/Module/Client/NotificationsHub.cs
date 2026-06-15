using Microsoft.AspNetCore.SignalR;

namespace CVTech.Modules.ActualiteEtAbonnement.Client;

/// <summary>
/// Hub SignalR des notifications in-app. Chaque client rejoint le groupe portant son
/// identifiant utilisateur ; les notifications lui sont poussées sur ce groupe.
/// </summary>
public sealed class NotificationsHub : Hub
{
    public async Task Rejoindre(string utilisateurId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, utilisateurId);
    }
}
