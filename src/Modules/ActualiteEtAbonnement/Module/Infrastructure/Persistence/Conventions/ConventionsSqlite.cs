using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CVTech.Modules.ActualiteEtAbonnement.Infrastructure.Persistence.Conventions;

/// <summary>
/// Compatibilité du provider de test SQLite (Azure SQL en production). SQLite ne sait pas
/// trier/filtrer nativement les <see cref="DateTimeOffset"/> : conversion en binaire triable.
/// Sans effet sur SqlServer.
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
