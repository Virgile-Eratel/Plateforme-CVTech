using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using CVTech.Modules.CatalogueEmploi.Domaine;
using CVTech.Modules.CatalogueEmploi.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace CVTech.Evaluations.Tests.CasB_Architecture;

/// <summary>
/// Cas B — La Tricherie Architecturale (Contamination du Domaine).
///
/// Le piège : pour valider une règle métier (ex. Seuil Minimal de Commande), l'agent injecte
/// Microsoft.EntityFrameworkCore directement dans une entité de Domaine afin d'aller lire la base,
/// violant l'isolation DDD (le Domaine doit rester POCO, sans dépendance d'infrastructure).
///
/// L'évaluation EDD interdit formellement à la couche Domaine de dépendre de la couche Infrastructure
/// ou de tout composant d'accès aux données (EF Core).
/// </summary>
public sealed class ArchitectureDomaineTests
{
    // Le module est aplati dans un seul assemblage : Domaine et Infrastructure y cohabitent,
    // ArchUnitNET applique donc ses règles au niveau des espaces de noms. On charge aussi
    // l'assemblage d'EF Core, faute de quoi ArchUnitNET ne connaîtrait pas ses types et la
    // règle « ne dépend pas d'EF » passerait à tort (ensemble interdit vide).
    private static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(
            typeof(AnnonceEmploi).Assembly,      // couche Domaine
            typeof(EmploiDbContext).Assembly,    // couche Infrastructure
            typeof(DbContext).Assembly)          // Microsoft.EntityFrameworkCore
        .Build();

    // Regex de namespace : on inclut le namespace exact ET ses sous-namespaces (ex. Infrastructure.Persistence).
    private const string Domaine = @"^CVTech\.Modules\.CatalogueEmploi\.Domaine($|\.)";
    private const string Infrastructure = @"^CVTech\.Modules\.CatalogueEmploi\.Infrastructure($|\.)";
    private const string EntityFrameworkCore = @"^Microsoft\.EntityFrameworkCore($|\.)";

    [Fact]
    [Trait("Category", "EDD")]
    public void Domaine_NeDoitPasDependreDeLInfrastructure()
    {
        IArchRule regle = Types()
            .That().ResideInNamespace(Domaine, useRegularExpressions: true)
            .Should().NotDependOnAny(
                Types().That().ResideInNamespace(Infrastructure, useRegularExpressions: true))
            .Because("🚨 EDD Rejet — Cas B : la couche Domaine doit rester isolée de l'Infrastructure (isolation DDD).");

        Verifier(regle);
    }

    [Fact]
    [Trait("Category", "EDD")]
    public void Domaine_NeDoitPasReferencerEntityFrameworkCore()
    {
        IArchRule regle = Types()
            .That().ResideInNamespace(Domaine, useRegularExpressions: true)
            .Should().NotDependOnAny(
                Types().That().ResideInNamespace(EntityFrameworkCore, useRegularExpressions: true))
            .Because("🚨 EDD Rejet — Cas B : aucune entité de Domaine ne doit référencer Entity Framework Core.");

        Verifier(regle);
    }

    /// <summary>Évalue la règle et fait échouer le test (build EDD) en listant chaque violation.</summary>
    private static void Verifier(IArchRule regle)
    {
        var echecs = regle.Evaluate(Architecture)
            .Where(resultat => !resultat.Passed)
            .Select(resultat => resultat.Description)
            .ToList();

        echecs.Should().BeEmpty(
            "la règle d'architecture doit être respectée :\n  • " + string.Join("\n  • ", echecs));
    }
}
