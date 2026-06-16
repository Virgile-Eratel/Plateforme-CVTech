using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace CVTech.Web.Services;

/// <summary>
/// Réception des notifications in-app en temps réel via le hub SignalR /hubs/notifications.
/// Se (re)connecte à chaque changement d'utilisateur courant, rejoint le groupe de l'utilisateur
/// (méthode serveur « Rejoindre ») et écoute la méthode « RecevoirNotification ».
/// </summary>
public sealed class ServiceNotifications : IAsyncDisposable
{
    private readonly SessionUtilisateur _session;
    private readonly string _urlHub;
    private HubConnection? _connexion;
    private Guid? _utilisateurConnecte;

    public ServiceNotifications(SessionUtilisateur session, NavigationManager navigation)
    {
        _session = session;
        _urlHub = navigation.ToAbsoluteUri("/hubs/notifications").ToString();
        _session.AuChangement += async () => await SynchroniserAsync();
    }

    /// <summary>Notifications reçues en temps réel pendant la session (les plus récentes en tête).</summary>
    public List<NotificationTempsReel> Recues { get; } = [];

    /// <summary>Levé à chaque notification reçue : permet à l'UI de rafraîchir badge et toast.</summary>
    public event Action<NotificationTempsReel>? AuMessage;

    public bool EstConnecte => _connexion?.State == HubConnectionState.Connected;

    private async Task SynchroniserAsync()
    {
        // L'utilisateur courant a changé : on coupe l'ancienne connexion et on rebranche.
        if (_session.UtilisateurId == _utilisateurConnecte) return;

        await ArreterAsync();

        if (_session.UtilisateurId is not { } id) return;

        _connexion = new HubConnectionBuilder()
            .WithUrl(_urlHub)
            .WithAutomaticReconnect()
            .Build();

        _connexion.On<NotificationTempsReel>("RecevoirNotification", notification =>
        {
            Recues.Insert(0, notification);
            AuMessage?.Invoke(notification);
        });

        // Rejoint son groupe à chaque (re)connexion, y compris après une reconnexion automatique.
        _connexion.Reconnected += async _ => await _connexion.InvokeAsync("Rejoindre", id.ToString());

        await _connexion.StartAsync();
        await _connexion.InvokeAsync("Rejoindre", id.ToString());
        _utilisateurConnecte = id;
    }

    private async Task ArreterAsync()
    {
        if (_connexion is not null)
        {
            await _connexion.DisposeAsync();
            _connexion = null;
        }
        _utilisateurConnecte = null;
        Recues.Clear();
    }

    public async ValueTask DisposeAsync() => await ArreterAsync();
}
