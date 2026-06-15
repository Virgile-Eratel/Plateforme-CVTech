namespace CVTech.SharedKernel.Permissions;

/// <summary>
/// Exception métier levée lorsqu'une action est refusée par la matrice de permissions.
/// Traduite en HTTP 403 par la couche Client.
/// </summary>
public sealed class PermissionRefuseeException(string message = "Action non autorisée.")
    : Exception(message);
