using CVTech.Modules.GestionIdentite.Domaine;
using FluentAssertions;
using Xunit;

namespace CVTech.Modules.GestionIdentite.Tests.Domaine;

public class UtilisateurTests
{
    [Fact]
    public void UneAdresseEmailInvalideEstRefuseeALinscription()
    {
        Action inscription = () => Utilisateur.Inscrire("pas-un-email", RoleUtilisateur.Candidat);

        inscription.Should().Throw<EmailInvalideException>();
    }

    [Fact]
    public void UnUtilisateurInscritNestPasBloque()
    {
        var utilisateur = Utilisateur.Inscrire("candidat@cvtech.fr", RoleUtilisateur.Candidat);

        utilisateur.EstBloque.Should().BeFalse();
        utilisateur.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void BloquerUnCandidatLeMarqueCommeBloque()
    {
        var candidat = Utilisateur.Inscrire("candidat@cvtech.fr", RoleUtilisateur.Candidat);

        candidat.Bloquer();

        candidat.EstBloque.Should().BeTrue();
    }

    [Fact]
    public void DefinirUnMotDePasseEnregistreSonEmpreinte()
    {
        var utilisateur = Utilisateur.Inscrire("candidat@cvtech.fr", RoleUtilisateur.Candidat);

        utilisateur.DefinirMotDePasse("empreinte-hachée");

        utilisateur.MotDePasseHash.Should().Be("empreinte-hachée");
    }

    [Fact]
    public void UneEmpreinteDeMotDePasseVideEstRefusee()
    {
        var utilisateur = Utilisateur.Inscrire("candidat@cvtech.fr", RoleUtilisateur.Candidat);

        Action action = () => utilisateur.DefinirMotDePasse("  ");

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UnAdministrateurNePeutPasEtreBloque()
    {
        var admin = Utilisateur.Creer("admin@cvtech.fr", RoleUtilisateur.Administrateur);

        Action blocage = () => admin.Bloquer();

        blocage.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void UnCompteAdministrateurNePeutPasEtreCreeParInscriptionPublique()
    {
        Action inscription = () => Utilisateur.Inscrire("admin@cvtech.fr", RoleUtilisateur.Administrateur);

        inscription.Should().Throw<RoleInscriptionInterditException>();
    }

    [Theory]
    [InlineData(RoleUtilisateur.Candidat)]
    [InlineData(RoleUtilisateur.Entreprise)]
    public void LinscriptionPubliqueAccepteLesRolesCandidatEtEntreprise(RoleUtilisateur role)
    {
        var utilisateur = Utilisateur.Inscrire("nouveau@cvtech.fr", role);

        utilisateur.Role.Should().Be(role);
    }

    [Fact]
    public void LeProvisionnementAdministratifPeutCreerUnAdministrateur()
    {
        var admin = Utilisateur.Creer("admin@cvtech.fr", RoleUtilisateur.Administrateur);

        admin.Role.Should().Be(RoleUtilisateur.Administrateur);
        admin.EstBloque.Should().BeFalse();
    }
}
