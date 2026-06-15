namespace CVTech.Modules.GestionIdentite.Domaine;

/// <summary>Exception métier : adresse e-mail d'inscription invalide.</summary>
public sealed class EmailInvalideException(string email)
    : Exception($"L'adresse e-mail « {email} » est invalide.");
