using CVTech.Modules.GestionIdentite.Domaine;
using CVTech.Modules.GestionIdentite.Infrastructure;
using CVTech.Modules.GestionIdentite.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CVTech.Modules.GestionIdentite.Tests.Infrastructure;

/// <summary>
/// Prouve que le dépôt EF Core persiste réellement (SQLite en mémoire, partagé par connexion).
/// Chaque assertion relit depuis un NOUVEau contexte pour ne pas dépendre du cache d'identité.
/// </summary>
public class UtilisateurRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connexion;

    public UtilisateurRepositoryTests()
    {
        _connexion = new SqliteConnection("DataSource=:memory:");
        _connexion.Open();
        using var contexte = CreerContexte();
        contexte.Database.EnsureCreated();
    }

    private IdentiteDbContext CreerContexte() =>
        new(new DbContextOptionsBuilder<IdentiteDbContext>().UseSqlite(_connexion).Options);

    [Fact]
    public async Task UnUtilisateurEnregistrePeutEtreReluDepuisLaBase()
    {
        var utilisateur = Utilisateur.Inscrire("candidat@cvtech.fr", RoleUtilisateur.Candidat);
        await using (var ctx = CreerContexte())
            await new UtilisateurRepository(ctx).AjouterAsync(utilisateur);

        await using var ctxLecture = CreerContexte();
        var relu = await new UtilisateurRepository(ctxLecture).ObtenirAsync(utilisateur.Id);

        relu.Should().NotBeNull();
        relu!.Email.Should().Be("candidat@cvtech.fr");
        relu.Role.Should().Be(RoleUtilisateur.Candidat);
        relu.EstBloque.Should().BeFalse();
    }

    [Fact]
    public async Task LeBlocageDUnCompteEstPersiste()
    {
        var utilisateur = Utilisateur.Inscrire("entreprise@cvtech.fr", RoleUtilisateur.Entreprise);
        await using (var ctx = CreerContexte())
        {
            var depot = new UtilisateurRepository(ctx);
            await depot.AjouterAsync(utilisateur);
            var charge = await depot.ObtenirAsync(utilisateur.Id);
            charge!.Bloquer();
            await depot.MettreAJourAsync(charge);
        }

        await using var ctxLecture = CreerContexte();
        var relu = await new UtilisateurRepository(ctxLecture).ObtenirAsync(utilisateur.Id);

        relu!.EstBloque.Should().BeTrue();
    }

    public void Dispose() => _connexion.Dispose();
}
