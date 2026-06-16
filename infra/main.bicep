// =============================================================================
// Plateforme-CVTech — Infrastructure Azure (ADR 0010)
// Azure SQL Database + App Service Linux (.NET 10) hébergeant l'API ET le front
// Blazor WebAssembly (un seul déployable). Les migrations EF Core s'appliquent au
// démarrage de l'application (InitialisationBdd.cs) quand Persistence:Provider=SqlServer.
// =============================================================================

@description('Préfixe de nommage des ressources (lettres minuscules/chiffres).')
param prefixe string = 'cvtech'

@description('Région de déploiement.')
param emplacement string = resourceGroup().location

@description('Identifiant administrateur du serveur Azure SQL.')
param adminSqlLogin string

@description('Mot de passe administrateur Azure SQL (variable secrète de pipeline).')
@secure()
param adminSqlPassword string

@description('Clé de signature des jetons JWT, >= 32 caractères (variable secrète de pipeline, ADR 0008).')
@secure()
param jwtCle string

@description('Référence (SKU) du plan App Service. F1 = gratuit (pas de alwaysOn).')
param skuPlan string = 'B1'

// alwaysOn n'est pas supporté par le tier gratuit F1 : on l'active seulement hors F1.
var alwaysOnActif = skuPlan != 'F1'

var nomServeurSql = '${prefixe}-sql-${uniqueString(resourceGroup().id)}'
var nomBaseSql = '${prefixe}-db'
var nomPlan = '${prefixe}-plan'
var nomApp = '${prefixe}-app-${uniqueString(resourceGroup().id)}'

// La chaîne pointe sur l'unique base : chaque module y crée son propre schéma
// (identite / emploi / freelance / actualite), conformément à l'ADR 0005.
var chaineConnexion = 'Server=tcp:${serveurSql.properties.fullyQualifiedDomainName},1433;Initial Catalog=${nomBaseSql};Persist Security Info=False;User ID=${adminSqlLogin};Password=${adminSqlPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'

// ----------------------------------------------------------------------------
// Azure SQL : serveur + base
// ----------------------------------------------------------------------------
resource serveurSql 'Microsoft.Sql/servers@2023-08-01-preview' = {
  name: nomServeurSql
  location: emplacement
  properties: {
    administratorLogin: adminSqlLogin
    administratorLoginPassword: adminSqlPassword
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
  }
}

resource baseSql 'Microsoft.Sql/servers/databases@2023-08-01-preview' = {
  parent: serveurSql
  name: nomBaseSql
  location: emplacement
  sku: {
    name: 'Basic'
    tier: 'Basic'
  }
}

// Autorise les services Azure (dont l'App Service) à joindre le serveur SQL.
resource regleParefeuAzure 'Microsoft.Sql/servers/firewallRules@2023-08-01-preview' = {
  parent: serveurSql
  name: 'AllowAllAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// ----------------------------------------------------------------------------
// App Service : plan Linux + application web (.NET 10)
// ----------------------------------------------------------------------------
resource plan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: nomPlan
  location: emplacement
  sku: {
    name: skuPlan
  }
  kind: 'linux'
  properties: {
    reserved: true // Linux
  }
}

resource app 'Microsoft.Web/sites@2023-12-01' = {
  name: nomApp
  location: emplacement
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|10.0'
      alwaysOn: alwaysOnActif
      ftpsState: 'Disabled'
      // Bascule la persistance sur Azure SQL : les migrations EF s'appliquent au démarrage.
      appSettings: [
        {
          name: 'Persistence__Provider'
          value: 'SqlServer'
        }
        {
          name: 'Jwt__Cle'
          value: jwtCle
        }
      ]
      connectionStrings: [
        {
          name: 'CVTech'
          connectionString: chaineConnexion
          type: 'SQLAzure'
        }
      ]
    }
  }
}

output nomApplication string = app.name
output urlApplication string = 'https://${app.properties.defaultHostName}'
output nomServeurSql string = serveurSql.name
