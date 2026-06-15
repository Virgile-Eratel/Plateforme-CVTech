using CVTech.Modules.GestionIdentite.Contracts;
using CVTech.Modules.GestionIdentite.Domaine;
using CVTech.Modules.GestionIdentite.Infrastructure;
using CVTech.SharedKernel.Permissions;
using FluentAssertions;
using Xunit;

namespace CVTech.Modules.GestionIdentite.Tests.Infrastructure;

public class VerificateurPermissionTests
{
    private static async Task<(VerificateurPermission verificateur, Utilisateur utilisateur)>
        PreparerAsync(RoleUtilisateur role, bool bloque = false)
    {
        var depot = new DepotUtilisateursEnMemoire();
        var utilisateur = Utilisateur.Inscrire($"{role}@cvtech.fr", role);
        if (bloque) utilisateur.Bloquer();
        await depot.AjouterAsync(utilisateur);
        return (new VerificateurPermission(depot), utilisateur);
    }

    [Fact]
    public async Task UnCandidatEstAutoriséÀPostuler()
    {
        var (verificateur, candidat) = await PreparerAsync(RoleUtilisateur.Candidat);

        var autorise = await verificateur.EstAutoriseAsync(candidat.Id, ActionMetier.PostulerAnnonce);

        autorise.Should().BeTrue();
    }

    [Fact]
    public async Task UneEntrepriseNestPasAutoriséeÀPostuler()
    {
        var (verificateur, entreprise) = await PreparerAsync(RoleUtilisateur.Entreprise);

        var autorise = await verificateur.EstAutoriseAsync(entreprise.Id, ActionMetier.PostulerAnnonce);

        autorise.Should().BeFalse();
    }

    [Fact]
    public async Task UnCompteBloquéNestAutoriséÀAucuneAction()
    {
        var (verificateur, candidat) = await PreparerAsync(RoleUtilisateur.Candidat, bloque: true);

        var autorise = await verificateur.EstAutoriseAsync(candidat.Id, ActionMetier.PostulerAnnonce);

        autorise.Should().BeFalse();
    }

    [Fact]
    public async Task ExigerLèvePermissionRefuséePourUnCompteInconnu()
    {
        var (verificateur, _) = await PreparerAsync(RoleUtilisateur.Candidat);

        Func<Task> action = () => verificateur.ExigerAsync(Guid.NewGuid(), ActionMetier.PostulerAnnonce);

        await action.Should().ThrowAsync<PermissionRefuseeException>();
    }
}
