using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace CVTech.Web.Services;

/// <summary>
/// Réception des notifications in-app en temps réel via le hub SignalR /hubs/notifications.
/// Le hub est authentifié (ADR 0008) : le jeton JWT est transmis via AccessTokenProvider, et
/// c'est le serveur qui rattache la connexion au groupe de l'utilisateur (d'après ses claims).
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

        if (_session is not { UtilisateurId: { } id, Jeton: { } }) return;

        _connexion = new HubConnectionBuilder()
            .WithUrl(_urlHub, options =>
                options.AccessTokenProvider = () => Task.FromResult<string?>(_session.Jeton))
            .WithAutomaticReconnect()
            .Build();

        _connexion.On<NotificationTempsReel>("RecevoirNotification", notification =>
        {
            Recues.Insert(0, notification);
            AuMessage?.Invoke(notification);
        });

        await _connexion.StartAsync();
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
