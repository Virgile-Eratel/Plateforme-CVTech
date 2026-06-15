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
public class DepotUtilisateursEfCoreTests : IDisposable
{
    private readonly SqliteConnection _connexion;

    public DepotUtilisateursEfCoreTests()
    {
        _connexion = new SqliteConnection("DataSource=:memory:");
        _connexion.Open();
        using var contexte = CreerContexte();
        contexte.Database.EnsureCreated();
    }

    private IdentiteDbContext CreerContexte() =>
        new(new DbContextOptionsBuilder<IdentiteDbContext>().UseSqlite(_connexion).Options);

    [Fact]
    public async Task UnUtilisateurEnregistréPeutÊtreReluDepuisLaBase()
    {
        var utilisateur = Utilisateur.Inscrire("candidat@cvtech.fr", RoleUtilisateur.Candidat);
        await using (var ctx = CreerContexte())
        {
            var depot = new DepotUtilisateursEfCore(ctx);
            await depot.AjouterAsync(utilisateur);
            await depot.EnregistrerAsync();
        }

        await using var ctxLecture = CreerContexte();
        var relu = await new DepotUtilisateursEfCore(ctxLecture).ObtenirAsync(utilisateur.Id);

        relu.Should().NotBeNull();
        relu!.Email.Should().Be("candidat@cvtech.fr");
        relu.Role.Should().Be(RoleUtilisateur.Candidat);
        relu.EstBloque.Should().BeFalse();
    }

    [Fact]
    public async Task LeBlocageDUnCompteEstPersisté()
    {
        var utilisateur = Utilisateur.Inscrire("entreprise@cvtech.fr", RoleUtilisateur.Entreprise);
        await using (var ctx = CreerContexte())
        {
            var depot = new DepotUtilisateursEfCore(ctx);
            await depot.AjouterAsync(utilisateur);
            await depot.EnregistrerAsync();
            var charge = await depot.ObtenirAsync(utilisateur.Id);
            charge!.Bloquer();
            await depot.EnregistrerAsync();
        }

        await using var ctxLecture = CreerContexte();
        var relu = await new DepotUtilisateursEfCore(ctxLecture).ObtenirAsync(utilisateur.Id);

        relu!.EstBloque.Should().BeTrue();
    }

    public void Dispose() => _connexion.Dispose();
}
