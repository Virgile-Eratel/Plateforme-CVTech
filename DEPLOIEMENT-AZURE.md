# ☁️ Déploiement sur Azure — guide pas à pas

Ce document explique **point par point** comment déployer Plateforme-CVTech sur Azure.

Il existe **deux chemins** :

- **[Chemin A — Déploiement manuel](#chemin-a--déploiement-manuel-az-cli)** (rapide, pour tester / démontrer).
- **[Chemin B — Déploiement automatisé](#chemin-b--déploiement-automatisé-azure-devops)** (le pipeline `azure-pipelines.yml`, livrable du TP).

Les deux s'appuient sur la même infrastructure décrite en Bicep (`infra/main.bicep`) :
**Azure SQL Database** + **App Service Linux (.NET 10)** qui héberge l'API **et** le front Blazor WASM.

> 🧠 **Principe clé** : à Azure, l'application démarre avec `Persistence:Provider=SqlServer` et la
> connection string `CVTech`. Au démarrage, elle **applique automatiquement les migrations EF Core**
> (`src/Api/InitialisationBdd.cs`) — il n'y a donc **aucun script SQL à lancer à la main**.

---

## 0. Prérequis (à faire une seule fois)

1. **Un abonnement Azure** actif (`az login` doit fonctionner).
2. **Azure CLI** installé, avec l'extension Bicep :
   ```bash
   az --version          # vérifier la présence
   az bicep install      # installe/maj le compilateur Bicep
   ```
3. **.NET 10 SDK** installé (`dotnet --version` → `10.x`).
4. **Pour le Chemin B uniquement** : une organisation **Azure DevOps** et l'agent self-hosted
   `vsts-agent-osx-arm64` (déjà présent dans le dossier parent) configuré et **en ligne**.

Définissez quelques variables réutilisées ci-dessous (adaptez les valeurs) :

```bash
GROUPE=rg-cvtech                 # nom du resource group
REGION=francecentral            # région Azure
PREFIXE=cvtech                  # préfixe de nommage des ressources
SQL_LOGIN=cvtechadmin           # login admin Azure SQL
SQL_PWD='ChoisirUnMotDePasseFort!2026'   # mot de passe admin (>= 8 car., complexe)
```

---

## Chemin A — Déploiement manuel (az CLI)

### A.1. Se connecter et choisir l'abonnement
```bash
az login
az account set --subscription "<NOM_OU_ID_ABONNEMENT>"
```

### A.2. Créer le resource group
```bash
az group create --name "$GROUPE" --location "$REGION"
```

### A.3. Provisionner l'infrastructure (Bicep)
Crée le serveur + base Azure SQL, la règle de pare-feu, le plan et l'App Service,
et **injecte déjà** `Persistence__Provider=SqlServer` + la connection string `CVTech`.
```bash
az deployment group create \
  --resource-group "$GROUPE" \
  --template-file infra/main.bicep \
  --parameters prefixe="$PREFIXE" \
               adminSqlLogin="$SQL_LOGIN" \
               adminSqlPassword="$SQL_PWD"
```

Récupérer le **nom** et l'**URL** de l'App Service générés :
```bash
NOM_APP=$(az deployment group show -g "$GROUPE" -n main \
  --query properties.outputs.nomApplication.value -o tsv)
URL_APP=$(az deployment group show -g "$GROUPE" -n main \
  --query properties.outputs.urlApplication.value -o tsv)
echo "App   : $NOM_APP"
echo "URL   : $URL_APP"
```

### A.4. Compiler et publier l'application
Le `publish` de l'API embarque automatiquement le front Blazor WASM (référence projet).
```bash
dotnet publish src/Api/CVTech.Api.csproj -c Release -o ./publish
```

### A.5. Empaqueter et déployer
```bash
cd publish && zip -r ../app.zip . && cd ..
az webapp deploy \
  --resource-group "$GROUPE" \
  --name "$NOM_APP" \
  --src-path app.zip \
  --type zip
```

### A.6. Vérifier
```bash
# Page d'accueil du front (HTTP 200 + HTML Blazor)
curl -I "$URL_APP"
# Flux RSS public
curl "$URL_APP/feed/rss"
```
Ouvrez `$URL_APP` dans un navigateur → l'application Blazor doit se charger.
Au premier appel, les **migrations EF Core** ont créé les schémas `identite`, `emploi`,
`freelance`, `actualite` dans la base Azure SQL.

> 💡 La première requête peut être lente (démarrage à froid + migrations). C'est normal.

---

## Chemin B — Déploiement automatisé (Azure DevOps)

Le pipeline `azure-pipelines.yml` enchaîne **restore → test → build → publish → deploy**.
Le déploiement n'a lieu **que si les tests sont au vert** (`dependsOn: Build_Test` + `condition: succeeded()`).

### B.1. Pousser le dépôt dans Azure DevOps
- Créez un **projet** dans votre organisation Azure DevOps.
- Poussez ce dépôt Git (`Repos`) ou connectez le dépôt GitHub existant.

### B.2. Préparer le pool d'agent self-hosted
- L'agent `vsts-agent-osx-arm64` doit être enregistré dans un **pool** et affiché **Online**
  (Project settings → **Agent pools**).
- Dans `azure-pipelines.yml`, alignez le nom du pool :
  ```yaml
  pool:
    name: Default        # ← remplacer par le nom réel de votre pool
  ```

### B.3. Créer la service connection ARM
- Project settings → **Service connections** → **New** → **Azure Resource Manager**.
- Portée : votre abonnement (et idéalement le resource group `rg-cvtech`).
- Notez le **nom** donné à la connection (ex. `sc-azure-cvtech`).

> ℹ️ Le pipeline ne crée pas le resource group : créez-le au préalable
> (`az group create -n rg-cvtech -l francecentral`) ou ajoutez l'étape correspondante.

### B.4. Définir les variables de pipeline
Pipeline → **Edit** → **Variables**, ajoutez :

| Variable | Exemple | Secrète |
|---|---|:---:|
| `serviceConnexionAzure` | `sc-azure-cvtech` | non |
| `groupeRessources` | `rg-cvtech` | non |
| `prefixe` | `cvtech` | non |
| `adminSqlLogin` | `cvtechadmin` | non |
| `adminSqlPassword` | `ChoisirUnMotDePasseFort!2026` | **oui** 🔒 |

> Cochez le **cadenas** pour `adminSqlPassword` afin qu'il soit masqué dans les logs.

### B.5. Créer l'environnement de déploiement
- Pipelines → **Environments** → **New environment** → nom **`cvtech-production`**
  (valeur attendue par le `deployment` du YAML). Type *None* suffit.

### B.6. Lancer le pipeline
- Pipelines → **New pipeline** → sélectionnez le dépôt → *Existing Azure Pipelines YAML file*
  → `/azure-pipelines.yml` → **Run**.

Déroulé attendu :
1. **Build_Test** : `dotnet restore`, `dotnet test` (45 tests), `dotnet build`,
   `dotnet publish` puis publication de l'artefact `cvtech-app`.
   ➡️ Si un test échoue, le pipeline s'arrête ici — **pas de déploiement**.
2. **Deploy** : `az deployment group create` (Bicep) puis `AzureWebApp@1` (déploie l'artefact).

### B.7. Vérifier
- Dans le portail Azure, ouvrez l'App Service → **Browse**, ou récupérez l'URL :
  ```bash
  az webapp show -g rg-cvtech -n <nomApp> --query defaultHostName -o tsv
  ```
- Testez `/feed/rss` et les 3 parcours via le front.

---

## 🔧 Détail des réglages appliqués automatiquement

| Réglage | Où | Valeur | Effet |
|---|---|---|---|
| `Persistence__Provider` | App settings (Bicep) | `SqlServer` | Bascule EF Core de SQLite vers Azure SQL |
| Connection string `CVTech` | App Service (Bicep, type `SQLAzure`) | `Server=...;Initial Catalog=cvtech-db;...` | Cible la base Azure SQL |
| `linuxFxVersion` | App Service (Bicep) | `DOTNETCORE\|10.0` | Runtime .NET 10 |
| Migrations EF | `InitialisationBdd.cs` au démarrage | `MigrateAsync()` | Crée/maj les schémas par module |
| Pare-feu SQL | Bicep | `0.0.0.0` (services Azure) | Autorise l'App Service à joindre SQL |

> En ASP.NET Core, une connection string définie dans l'App Service sous le nom `CVTech`
> est automatiquement exposée comme `ConnectionStrings:CVTech` — c'est exactement ce que lit
> `Program.cs`. Aucune valeur secrète n'est donc en clair dans le code.

---

## 🩺 Dépannage

| Symptôme | Cause probable | Action |
|---|---|---|
| `Login failed for user` au démarrage | Pare-feu SQL ou identifiants | Vérifier la règle `AllowAllAzureServices` et `adminSqlLogin/Password` |
| HTTP 500 au 1ᵉʳ appel | Migrations en cours / échec | Consulter les logs : `az webapp log tail -g rg-cvtech -n <nomApp>` |
| Le front se charge mais les appels API échouent | Mauvaise origine / HTTPS | Vérifier que l'API et le front sont bien sur la **même** URL (hébergés ensemble) |
| Pipeline bloqué sur l'agent | Pool incorrect ou agent offline | Aligner `pool.name` et vérifier l'état de l'agent |
| `BicepParameterTypeMismatch` | Paramètre manquant | Vérifier `adminSqlLogin` / `adminSqlPassword` dans les variables |

Logs en direct :
```bash
az webapp log tail --resource-group rg-cvtech --name <nomApp>
```

---

## 🧹 Suppression des ressources (éviter les coûts)

```bash
az group delete --name rg-cvtech --yes --no-wait
```

---

## ⚠️ Avant une mise en production réelle

L'authentification JWT n'est pas encore branchée (ADR 0008) : l'identité est aujourd'hui portée
par le client. **Avant tout usage réel**, finaliser l'auth (dériver l'identité de `HttpContext.User`)
et **restreindre le CORS** (actuellement permissif). Tant que ce n'est pas fait, réservez le
déploiement à une démonstration.
