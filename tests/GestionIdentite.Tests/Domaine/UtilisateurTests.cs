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
        var admin = Utilisateur.Inscrire("admin@cvtech.fr", RoleUtilisateur.Administrateur);

        Action blocage = () => admin.Bloquer();

        blocage.Should().Throw<InvalidOperationException>();
    }
}
