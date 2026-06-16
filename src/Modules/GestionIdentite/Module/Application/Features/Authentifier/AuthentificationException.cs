namespace CVTech.Modules.GestionIdentite.Application.Features.Authentifier;

/// <summary>
/// Échec d'authentification (identifiants invalides ou compte bloqué).
/// Traduite en HTTP 401 par la couche Client. Message volontairement générique
/// pour ne pas révéler si l'e-mail existe.
/// </summary>
public sealed class AuthentificationException(string message = "Identifiants invalides.")
    : Exception(message);
