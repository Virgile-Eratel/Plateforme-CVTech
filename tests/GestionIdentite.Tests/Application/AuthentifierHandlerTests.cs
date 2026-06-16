using CVTech.Modules.GestionIdentite.Application;
using CVTech.Modules.GestionIdentite.Application.Features.Authentifier;
using CVTech.Modules.GestionIdentite.Domaine;
using FluentAssertions;
using Xunit;

namespace CVTech.Modules.GestionIdentite.Tests.Application;

public class AuthentifierHandlerTests
{
    /// <summary>Faux hacheur : « hash » = "h:" + clair. Déterministe, sans dépendance crypto.</summary>
    private sealed class HacheurFactice : IHacheurMotDePasse
    {
        public string Hacher(string motDePasseClair) => "h:" + motDePasseClair;
        public bool Verifier(string hash, string motDePasseClair) => hash == "h:" + motDePasseClair;
    }

    private sealed class DepotFactice : IDepotUtilisateurs
    {
        private readonly List<Utilisateur> _u = [];
        public Task AjouterAsync(Utilisateur u, CancellationToken ct = default) { _u.Add(u); return Task.CompletedTask; }
        public Task<Utilisateur?> ObtenirAsync(Guid id, CancellationToken ct = default) =>
            Task.FromResult(_u.FirstOrDefault(x => x.Id == id));
        public Task<Utilisateur?> ObtenirParEmailAsync(string email, CancellationToken ct = default) =>
            Task.FromResult(_u.FirstOrDefault(x => x.Email == email));
        public Task EnregistrerAsync(CancellationToken ct = default) => Task.CompletedTask;
    }

    private static Utilisateur CreerAvecMotDePasse(string email, RoleUtilisateur role, string motDePasse)
    {
        var u = Utilisateur.Inscrire(email, role);
        u.DefinirMotDePasse("h:" + motDePasse);
        return u;
    }

    [Fact]
    public async Task UnUtilisateurAvecLesBonsIdentifiantsEstAuthentifie()
    {
        var depot = new DepotFactice();
        await depot.AjouterAsync(CreerAvecMotDePasse("candidat@cvtech.fr", RoleUtilisateur.Candidat, "Secret123"));
        var handler = new AuthentifierHandler(depot, new HacheurFactice());

        var resultat = await handler.Handle(
            new AuthentifierCommand("candidat@cvtech.fr", "Secret123"), CancellationToken.None);

        resultat.Email.Should().Be("candidat@cvtech.fr");
        resultat.Role.Should().Be(RoleUtilisateur.Candidat);
    }

    [Fact]
    public async Task UnMotDePasseIncorrectEstRefuse()
    {
        var depot = new DepotFactice();
        await depot.AjouterAsync(CreerAvecMotDePasse("candidat@cvtech.fr", RoleUtilisateur.Candidat, "Secret123"));
        var handler = new AuthentifierHandler(depot, new HacheurFactice());

        Func<Task> action = () => handler.Handle(
            new AuthentifierCommand("candidat@cvtech.fr", "MauvaisMdp"), CancellationToken.None);

        await action.Should().ThrowAsync<AuthentificationException>();
    }

    [Fact]
    public async Task UnEmailInconnuEstRefuse()
    {
        var handler = new AuthentifierHandler(new DepotFactice(), new HacheurFactice());

        Func<Task> action = () => handler.Handle(
            new AuthentifierCommand("inconnu@cvtech.fr", "Secret123"), CancellationToken.None);

        await action.Should().ThrowAsync<AuthentificationException>();
    }

    [Fact]
    public async Task UnCompteBloqueNePeutPasSAuthentifier()
    {
        var depot = new DepotFactice();
        var compte = CreerAvecMotDePasse("candidat@cvtech.fr", RoleUtilisateur.Candidat, "Secret123");
        compte.Bloquer();
        await depot.AjouterAsync(compte);
        var handler = new AuthentifierHandler(depot, new HacheurFactice());

        Func<Task> action = () => handler.Handle(
            new AuthentifierCommand("candidat@cvtech.fr", "Secret123"), CancellationToken.None);

        await action.Should().ThrowAsync<AuthentificationException>();
    }
}
