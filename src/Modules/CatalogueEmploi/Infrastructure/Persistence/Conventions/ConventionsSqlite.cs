using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CVTech.Modules.CatalogueEmploi.Infrastructure.Persistence.Conventions;

/// <summary>
/// Compatibilité du provider de test SQLite (utilisé par les tests ; Azure SQL en production).
/// SQLite ne sait pas trier/filtrer nativement les <see cref="DateTimeOffset"/> : on les convertit
/// alors en binaire triable. Sans effet sur SqlServer, qui les gère nativement.
/// </summary>
public static class ConventionsSqlite
{
    private const string ProviderSqlite = "Microsoft.EntityFrameworkCore.Sqlite";

    public static void AppliquerSi(DbContext contexte, ModelConfigurationBuilder configurationBuilder)
    {
        if (contexte.Database.ProviderName == ProviderSqlite)
            configurationBuilder.Properties<DateTimeOffset>()
                .HaveConversion<DateTimeOffsetToBinaryConverter>();
    }
}
