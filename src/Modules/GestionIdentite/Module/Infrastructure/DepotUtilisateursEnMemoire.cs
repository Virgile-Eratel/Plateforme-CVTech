using System.Collections.Concurrent;
using CVTech.Modules.GestionIdentite.Application;
using CVTech.Modules.GestionIdentite.Domaine;

namespace CVTech.Modules.GestionIdentite.Infrastructure;

/// <summary>
/// Dépôt en mémoire pour le module pilote. Sera remplacé par une implémentation
/// EF Core / Azure SQL en Phase 3 (ADR 0005) sans toucher à l'Application.
/// </summary>
public sealed class DepotUtilisateursEnMemoire : IDepotUtilisateurs
{
    private readonly ConcurrentDictionary<Guid, Utilisateur> _utilisateurs = new();

    public Task AjouterAsync(Utilisateur utilisateur, CancellationToken ct = default)
    {
        _utilisateurs[utilisateur.Id] = utilisateur;
        return Task.CompletedTask;
    }

    public Task<Utilisateur?> ObtenirAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(_utilisateurs.GetValueOrDefault(id));

    public Task EnregistrerAsync(CancellationToken ct = default) => Task.CompletedTask;
}
