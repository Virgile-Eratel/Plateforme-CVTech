using CVTech.Modules.GestionIdentite.Domaine;
using FluentAssertions;
using Xunit;

namespace CVTech.Modules.GestionIdentite.Tests.Domaine;

public class UtilisateurTests
{
    [Fact]
    public void UneAdresseEmailInvalideEstRefuséeÀLinscription()
    {
        Action inscription = () => Utilisateur.Inscrire("pas-un-email", RoleUtilisateur.Candidat);

        inscription.Should().Throw<EmailInvalideException>();
    }

    [Fact]
    public void UnUtilisateurInscritNestPasBloqué()
    {
        var utilisateur = Utilisateur.Inscrire("candidat@cvtech.fr", RoleUtilisateur.Candidat);

        utilisateur.EstBloque.Should().BeFalse();
        utilisateur.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void BloquerUnCandidatLeMarqueCommeBloqué()
    {
        var candidat = Utilisateur.Inscrire("candidat@cvtech.fr", RoleUtilisateur.Candidat);

        candidat.Bloquer();

        candidat.EstBloque.Should().BeTrue();
    }

    [Fact]
    public void DéfinirUnMotDePasseEnregistreSonEmpreinte()
    {
        var utilisateur = Utilisateur.Inscrire("candidat@cvtech.fr", RoleUtilisateur.Candidat);

        utilisateur.DefinirMotDePasse("empreinte-hachée");

        utilisateur.MotDePasseHash.Should().Be("empreinte-hachée");
    }

    [Fact]
    public void UneEmpreinteDeMotDePasseVideEstRefusée()
    {
        var utilisateur = Utilisateur.Inscrire("candidat@cvtech.fr", RoleUtilisateur.Candidat);

        Action action = () => utilisateur.DefinirMotDePasse("  ");

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UnAdministrateurNePeutPasÊtreBloqué()
    {
        var admin = Utilisateur.Creer("admin@cvtech.fr", RoleUtilisateur.Administrateur);

        Action blocage = () => admin.Bloquer();

        blocage.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void UnCompteAdministrateurNePeutPasÊtreCrééParInscriptionPublique()
    {
        Action inscription = () => Utilisateur.Inscrire("admin@cvtech.fr", RoleUtilisateur.Administrateur);

        inscription.Should().Throw<RoleInscriptionInterditException>();
    }

    [Theory]
    [InlineData(RoleUtilisateur.Candidat)]
    [InlineData(RoleUtilisateur.Entreprise)]
    public void LinscriptionPubliqueAccepteLesRôlesCandidatEtEntreprise(RoleUtilisateur role)
    {
        var utilisateur = Utilisateur.Inscrire("nouveau@cvtech.fr", role);

        utilisateur.Role.Should().Be(role);
    }

    [Fact]
    public void LeProvisionnementAdministratifPeutCréerUnAdministrateur()
    {
        var admin = Utilisateur.Creer("admin@cvtech.fr", RoleUtilisateur.Administrateur);

        admin.Role.Should().Be(RoleUtilisateur.Administrateur);
        admin.EstBloque.Should().BeFalse();
    }
}
