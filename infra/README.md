# Infrastructure Azure — Plateforme-CVTech

IaC Bicep (ADR 0010) : **Azure SQL Database** + **App Service Linux (.NET 10)** hébergeant
l'API et le front Blazor WebAssembly (un seul déployable).

## Ressources créées (`main.bicep`)
- `Microsoft.Sql/servers` + `databases` (Basic) — une base, un schéma par module (ADR 0005).
- Règle de pare-feu autorisant les services Azure.
- `Microsoft.Web/serverfarms` (Linux) + `Microsoft.Web/sites` avec :
  - app setting `Persistence__Provider=SqlServer` ;
  - connection string `CVTech` (type `SQLAzure`).

Au démarrage, l'application applique les **migrations EF Core** sur Azure SQL
(`InitialisationBdd.cs`, branche `SqlServer`).

## Déploiement manuel (test rapide)
```bash
az group create -n rg-cvtech -l francecentral

az deployment group create \
  -g rg-cvtech \
  --template-file infra/main.bicep \
  --parameters prefixe=cvtech \
               adminSqlLogin=cvtechadmin \
               adminSqlPassword='<MotDePasseFort!>'

# Récupère l'URL :
az deployment group show -g rg-cvtech -n main --query properties.outputs.urlApplication.value -o tsv
```

Puis publier et déployer le binaire :
```bash
dotnet publish src/Api/CVTech.Api.csproj -c Release -o ./publish
cd publish && zip -r ../app.zip . && cd ..
az webapp deploy -g rg-cvtech -n <nomApp> --src-path app.zip --type zip
```

## Déploiement automatisé
Voir `azure-pipelines.yml` (restore → test → build → publish → deploy ; deploy uniquement
si les tests sont verts). Variables de pipeline à définir :

| Variable | Description | Secrète |
|---|---|:---:|
| `serviceConnexionAzure` | Service connection ARM | non |
| `groupeRessources` | Resource group cible | non |
| `prefixe` | Préfixe de nommage (ex. `cvtech`) | non |
| `adminSqlLogin` | Login admin Azure SQL | non |
| `adminSqlPassword` | Mot de passe admin Azure SQL | **oui** |
