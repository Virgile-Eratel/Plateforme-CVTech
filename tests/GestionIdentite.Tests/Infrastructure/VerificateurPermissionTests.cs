using CVTech.Modules.GestionIdentite.Application;
using CVTech.Modules.GestionIdentite.Contracts;
using CVTech.Modules.GestionIdentite.Domaine;
using CVTech.Modules.GestionIdentite.Infrastructure;
using CVTech.SharedKernel.Permissions;
using FluentAssertions;
using Xunit;

namespace CVTech.Modules.GestionIdentite.Tests.Infrastructure;

public class VerificateurPermissionTests
{
    /// <summary>Faux dépôt minimal : le VerificateurPermission n'a besoin que d'ObtenirAsync.</summary>
    private sealed class DepotUtilisateursFactice : IDepotUtilisateurs
    {
        private readonly Dictionary<Guid, Utilisateur> _utilisateurs = new();
        public Task AjouterAsync(Utilisateur utilisateur, CancellationToken ct = default)
        {
            _utilisateurs[utilisateur.Id] = utilisateur;
            return Task.CompletedTask;
        }
        public Task<Utilisateur?> ObtenirAsync(Guid id, CancellationToken ct = default) =>
            Task.FromResult(_utilisateurs.GetValueOrDefault(id));
        public Task<Utilisateur?> ObtenirParEmailAsync(string email, CancellationToken ct = default) =>
            Task.FromResult(_utilisateurs.Values.FirstOrDefault(u => u.Email == email));
        public Task EnregistrerAsync(CancellationToken ct = default) => Task.CompletedTask;
    }

    private static async Task<(VerificateurPermission verificateur, Utilisateur utilisateur)>
        PreparerAsync(RoleUtilisateur role, bool bloque = false)
    {
        var depot = new DepotUtilisateursFactice();
        var utilisateur = Utilisateur.Inscrire($"{role}@cvtech.fr", role);
        if (bloque) utilisateur.Bloquer();
        await depot.AjouterAsync(utilisateur);
        return (new VerificateurPermission(depot), utilisateur);
    }

    [Fact]
    public async Task UnCandidatEstAutoriseAPostuler()
    {
        var (verificateur, candidat) = await PreparerAsync(RoleUtilisateur.Candidat);

        var autorise = await verificateur.EstAutoriseAsync(candidat.Id, ActionMetier.PostulerAnnonce);

        autorise.Should().BeTrue();
    }

    [Fact]
    public async Task UneEntrepriseNestPasAutoriseeAPostuler()
    {
        var (verificateur, entreprise) = await PreparerAsync(RoleUtilisateur.Entreprise);

        var autorise = await verificateur.EstAutoriseAsync(entreprise.Id, ActionMetier.PostulerAnnonce);

        autorise.Should().BeFalse();
    }

    [Fact]
    public async Task UnCompteBloqueNestAutoriseAAucuneAction()
    {
        var (verificateur, candidat) = await PreparerAsync(RoleUtilisateur.Candidat, bloque: true);

        var autorise = await verificateur.EstAutoriseAsync(candidat.Id, ActionMetier.PostulerAnnonce);

        autorise.Should().BeFalse();
    }

    [Fact]
    public async Task ExigerLevePermissionRefuseePourUnCompteInconnu()
    {
        var (verificateur, _) = await PreparerAsync(RoleUtilisateur.Candidat);

        Func<Task> action = () => verificateur.ExigerAsync(Guid.NewGuid(), ActionMetier.PostulerAnnonce);

        await action.Should().ThrowAsync<PermissionRefuseeException>();
    }
}
