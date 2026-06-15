using CVTech.Modules.GestionIdentite.Contracts;
using CVTech.Modules.GestionIdentite.Domaine;
using FluentAssertions;
using Xunit;

namespace CVTech.Modules.GestionIdentite.Tests.Domaine;

public class MatricePermissionsTests
{
    [Theory]
    [InlineData(RoleUtilisateur.Candidat, ActionMetier.PostulerAnnonce, true)]
    [InlineData(RoleUtilisateur.Candidat, ActionMetier.PublierAnnonce, false)]
    [InlineData(RoleUtilisateur.Entreprise, ActionMetier.PublierAnnonce, true)]
    [InlineData(RoleUtilisateur.Entreprise, ActionMetier.PostulerAnnonce, false)]
    [InlineData(RoleUtilisateur.Administrateur, ActionMetier.ModererAnnonceOuAppelOffre, true)]
    [InlineData(RoleUtilisateur.Candidat, ActionMetier.ModererAnnonceOuAppelOffre, false)]
    public void LaMatriceRespecteLeReadme(RoleUtilisateur role, ActionMetier action, bool attendu)
    {
        MatricePermissions.EstAutorise(role, action).Should().Be(attendu);
    }

    [Fact]
    public void UnAdministrateurHériteDeTousLesDroits()
    {
        foreach (var action in Enum.GetValues<ActionMetier>())
            MatricePermissions.EstAutorise(RoleUtilisateur.Administrateur, action).Should().BeTrue();
    }
}
