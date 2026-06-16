# ☁️ Déploiement sur Azure — guide pas à pas

Ce document explique **point par point** comment déployer Plateforme-CVTech sur Azure,
**du premier coup**, y compris après avoir **supprimé entièrement le resource group**.

Il existe **deux chemins** :

- **[Chemin A — Déploiement manuel](#chemin-a--déploiement-manuel-az-cli)** (rapide, pour tester / démontrer).
- **[Chemin B — Déploiement automatisé](#chemin-b--déploiement-automatisé-azure-devops)** (le pipeline `azure-pipelines.yml`, livrable du TP).

Les deux s'appuient sur la même infrastructure décrite en Bicep (`infra/main.bicep`) :
**Azure SQL Database** + **App Service Linux (.NET 10)** qui héberge l'API **et** le front Blazor WASM.

> 🧠 **Principe clé** : à Azure, l'application démarre avec `Persistence:Provider=SqlServer` et la
> connection string `CVTech`. Au démarrage, elle **applique automatiquement les migrations EF Core**
> (`src/Api/InitialisationBdd.cs`) — il n'y a donc **aucun script SQL à lancer à la main**.

> ✅ **Trois réglages critiques sont déjà gérés** et ne demandent **aucune action manuelle** :
> 1. **Commande de démarrage** (`appCommandLine: dotnet CVTech.Api.dll`) → dans le Bicep.
>    Sans elle, App Service afficherait sa page par défaut « *waiting for your content* ».
> 2. **Front Blazor empreinté** : une cible MSBuild (`ResoudreFrontBlazor` dans `CVTech.Api.csproj`)
>    régénère `index.html` + l'import map + les assets `_framework` cohérents au `dotnet publish`.
>    Sans elle, le front renvoyait des **404** sur `blazor.webassembly`/`dotnet.*.js`.
> 3. **Tier `F1` (gratuit)** : imposé par les abonnements **Azure for Students** (voir ⚠️ ci-dessous).

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

Définissez les variables réutilisées ci-dessous (adaptez les valeurs) :

```bash
GROUPE=rg-cvtech-fc              # nom du resource group
REGION=francecentral            # région Azure (voir ⚠️ capacité ci-dessous)
PREFIXE=cvtech                  # préfixe de nommage des ressources
SQL_LOGIN=cvtechadmin           # login admin Azure SQL
SQL_PWD='Cvtech!2026Azure#Db'   # mot de passe admin (voir 🔑 règles ci-dessous)
JWT_CLE='cle-jwt-aleatoire-de-32-caracteres-minimum-xyz'   # clé de signature JWT (ADR 0008)
SKU_PLAN=F1                     # F1 (gratuit) pour Azure for Students ; B1 sinon
```

> 🔑 **Règles du mot de passe SQL** (sinon erreur `PasswordNotComplex`) : 8 à 128 caractères,
> **au moins 3 des 4** catégories (majuscule, minuscule, chiffre, caractère spécial), et il **ne
> doit pas contenir le login**. Utilisez des **guillemets simples** en shell pour protéger `! $ #`.

> ⚠️ **Capacité App Service & Azure for Students** : sur un abonnement étudiant, le tier **B1
> Linux** renvoie très souvent `Conflict — No available instances` (pool saturé), **même sur un
> resource group neuf**. Utilisez le tier **gratuit `F1`** (`SKU_PLAN=F1`), dont le pool est
> quasi toujours disponible. Sur un abonnement payant classique, `B1` fonctionne et active
> `alwaysOn` (pas de mise en veille). Le Bicep gère automatiquement `alwaysOn` (désactivé sur F1).

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
Crée le serveur + base Azure SQL, la règle de pare-feu, le plan et l'App Service. Injecte déjà
`Persistence__Provider=SqlServer`, la connection string `CVTech`, et la **commande de démarrage**.
```bash
az deployment group create \
  --resource-group "$GROUPE" \
  --template-file infra/main.bicep \
  --parameters prefixe="$PREFIXE" \
               adminSqlLogin="$SQL_LOGIN" \
               adminSqlPassword="$SQL_PWD" \
               jwtCle="$JWT_CLE" \
               skuPlan="$SKU_PLAN"
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
Le `publish` de l'API embarque automatiquement le front Blazor WASM (référence projet) **et**
régénère un `index.html` cohérent (cible MSBuild `ResoudreFrontBlazor`) — rien d'autre à faire.
```bash
rm -rf publish app.zip
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

> La commande de démarrage est déjà posée par le Bicep. Si jamais l'app affiche la page par
> défaut d'Azure, forcez-la manuellement (puis `az webapp restart`) :
> ```bash
> az webapp config set -g "$GROUPE" -n "$NOM_APP" --startup-file "dotnet CVTech.Api.dll"
> ```

### A.6. Vérifier
```bash
curl -I "$URL_APP"                                   # 200 + HTML Blazor
curl "$URL_APP/feed/rss"                             # flux RSS public
curl -o /dev/null -w "%{http_code}\n" "$URL_APP/_framework/blazor.boot.json"  # 200 attendu
```
Ouvrez `$URL_APP` dans un navigateur (**Cmd+Shift+R** pour vider le cache) → l'application Blazor
doit se charger. Au premier appel, les **migrations EF Core** créent les schémas `identite`,
`emploi`, `freelance`, `actualite` dans la base Azure SQL.

> 💡 Sur **F1**, `alwaysOn` est désactivé : la première requête après une période d'inactivité est
> lente (démarrage à froid + migrations). C'est normal.

---

## A.7. (Optionnel) Injecter des données de démo réalistes

L'outil `tools/CVTech.Seeder` (Bogus, locale fr) peuple la base avec ~56 comptes, des annonces,
appels d'offres, propositions, articles, abonnements et notifications cohérents. Il est
**idempotent** (ne réinsère pas si la base est déjà peuplée) et se connecte **directement** à
Azure SQL — il faut donc autoriser temporairement votre IP au pare-feu SQL.

```bash
SQL_SRV=$(az deployment group show -g "$GROUPE" -n main \
  --query properties.outputs.nomServeurSql.value -o tsv)

# 1. Autoriser votre IP publique sur le serveur SQL
MONIP=$(curl -s ifconfig.me)
az sql server firewall-rule create -g "$GROUPE" -s "$SQL_SRV" \
  -n poste-seed --start-ip-address "$MONIP" --end-ip-address "$MONIP"

# 2. Lancer le seeder
CONN="Server=tcp:${SQL_SRV}.database.windows.net,1433;Initial Catalog=${PREFIXE}-db;User ID=$SQL_LOGIN;Password=$SQL_PWD;Encrypt=True;TrustServerCertificate=False;Connection Timeout=60;"
dotnet run --project tools/CVTech.Seeder -- "$CONN"

# 3. (propreté) retirer la règle pare-feu
az sql server firewall-rule delete -g "$GROUPE" -s "$SQL_SRV" -n poste-seed
```

Options : `--force` (vide les tables puis régénère) · `--dry-run` (génère sans écrire en base).

**Comptes de démo générés** (mot de passe `Demo!2026`) :

| Email | Rôle |
|---|---|
| `admin@cvtech.fr` | Administrateur |
| `entreprise@cvtech.fr` | Entreprise |
| `candidat@cvtech.fr` | Candidat |

> L'outil est volontairement **hors** `CVTech.slnx` (n'alourdit pas la CI). Il référence les
> modules comme `src/Api` et appelle les méthodes factory du Domaine (invariants respectés).

---

## Chemin B — Déploiement automatisé (Azure DevOps)

Le pipeline `azure-pipelines.yml` enchaîne **restore → test → build → publish → deploy**.
Le déploiement n'a lieu **que si les tests sont au vert** (`dependsOn: Build_Test` + `condition: succeeded()`).
La cible MSBuild `ResoudreFrontBlazor` s'exécute aussi en CI (même `dotnet publish`) → le front est correct.

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
- Portée : votre abonnement (et idéalement le resource group `rg-cvtech-fc`).
- Notez le **nom** donné à la connection (ex. `sc-azure-cvtech`).

> ℹ️ Le pipeline ne crée pas le resource group : créez-le au préalable
> (`az group create -n rg-cvtech-fc -l francecentral`) ou ajoutez l'étape correspondante.

### B.4. Définir les variables de pipeline
Pipeline → **Edit** → **Variables**, ajoutez :

| Variable | Exemple | Secrète |
|---|---|:---:|
| `serviceConnexionAzure` | `sc-azure-cvtech` | non |
| `groupeRessources` | `rg-cvtech-fc` | non |
| `prefixe` | `cvtech` | non |
| `skuPlan` | `F1` (étudiant) / `B1` | non |
| `adminSqlLogin` | `cvtechadmin` | non |
| `adminSqlPassword` | `Cvtech!2026Azure#Db` | **oui** 🔒 |
| `jwtCle` | chaîne aléatoire ≥ 32 caractères | **oui** 🔒 |

> Cochez le **cadenas** pour `adminSqlPassword` et `jwtCle` afin qu'ils soient masqués dans les logs.
> Assurez-vous que le `az deployment group create` du YAML passe bien `skuPlan=$(skuPlan)`.

### B.5. Créer l'environnement de déploiement
- Pipelines → **Environments** → **New environment** → nom **`cvtech-production`**
  (valeur attendue par le `deployment` du YAML). Type *None* suffit.

### B.6. Lancer le pipeline
- Pipelines → **New pipeline** → sélectionnez le dépôt → *Existing Azure Pipelines YAML file*
  → `/azure-pipelines.yml` → **Run**.

Déroulé attendu :
1. **Build_Test** : `dotnet restore`, `dotnet test`, `dotnet build`, `dotnet publish` puis
   publication de l'artefact `cvtech-app`. ➡️ Si un test échoue, **pas de déploiement**.
2. **Deploy** : `az deployment group create` (Bicep) puis `AzureWebApp@1` (déploie l'artefact).

### B.7. Vérifier
- Dans le portail Azure, ouvrez l'App Service → **Browse**, ou récupérez l'URL :
  ```bash
  az webapp show -g rg-cvtech-fc -n <nomApp> --query defaultHostName -o tsv
  ```
- Testez `/feed/rss` et les 3 parcours via le front.

---

## 🔧 Détail des réglages appliqués automatiquement

| Réglage | Où | Valeur | Effet |
|---|---|---|---|
| `Persistence__Provider` | App settings (Bicep) | `SqlServer` | Bascule EF Core de SQLite vers Azure SQL |
| Connection string `CVTech` | App Service (Bicep, type `SQLAzure`) | `Server=...;Initial Catalog=cvtech-db;...` | Cible la base Azure SQL |
| `linuxFxVersion` | App Service (Bicep) | `DOTNETCORE\|10.0` | Runtime .NET 10 |
| `appCommandLine` | App Service (Bicep) | `dotnet CVTech.Api.dll` | Point d'entrée explicite (évite la page par défaut) |
| `alwaysOn` | App Service (Bicep) | `true` sauf F1 | Évite la mise en veille (désactivé sur le tier gratuit) |
| Front empreinté | `CVTech.Api.csproj` (cible `ResoudreFrontBlazor`) | au `publish` | `index.html` + import map + `_framework` cohérents |
| Migrations EF | `InitialisationBdd.cs` au démarrage | `MigrateAsync()` | Crée/maj les schémas par module |
| Pare-feu SQL | Bicep | `0.0.0.0` (services Azure) | Autorise l'App Service à joindre SQL |

> En ASP.NET Core, une connection string définie dans l'App Service sous le nom `CVTech`
> est automatiquement exposée comme `ConnectionStrings:CVTech` — c'est exactement ce que lit
> `Program.cs`. Aucune valeur secrète n'est donc en clair dans le code.

---

## 🩺 Dépannage

| Symptôme | Cause | Action |
|---|---|---|
| `PasswordNotComplex` au déploiement Bicep | Mot de passe SQL trop faible / mal échappé | Respecter les règles 🔑, mettre la valeur entre **guillemets simples** |
| `Conflict — No available instances` (serverfarms) | Pool App Service saturé (fréquent sur Azure for Students en B1) | Déployer en **`skuPlan=F1`** ; sinon changer de région ou réessayer |
| Page « *waiting for your content* » / `Content root: /defaulthome/hostingstart/` | Commande de démarrage absente | Vérifier `appCommandLine` (Bicep) ou `az webapp config set --startup-file "dotnet CVTech.Api.dll"` |
| **404** sur `_framework/blazor.webassembly` ou `dotnet.*.js` | `index.html` non régénéré au publish (cible MSBuild absente) | Republier avec le code à jour (cible `ResoudreFrontBlazor`) ; vider le cache navigateur |
| `Login failed for user` au démarrage | Pare-feu SQL ou identifiants | Vérifier `AllowAllAzureServices` et `adminSqlLogin/Password` |
| HTTP 500 au 1ᵉʳ appel | Migrations en cours / échec | `az webapp log tail -g $GROUPE -n <nomApp>` |
| Seeder : `Cannot open server ... requested by the login` | IP non autorisée au pare-feu SQL | Ajouter la règle pare-feu pour votre IP (cf. A.7) |
| Pipeline bloqué sur l'agent | Pool incorrect ou agent offline | Aligner `pool.name` et vérifier l'état de l'agent |

Logs en direct :
```bash
az webapp log tail --resource-group "$GROUPE" --name "$NOM_APP"
```

---

## 🧹 Suppression des ressources (éviter les coûts)

```bash
az group delete --name "$GROUPE" --yes --no-wait
```

> Tout est reproductible : il suffit de relancer **A.2 → A.6** (puis A.7) pour redéployer
> entièrement, du premier coup.

---

## 🔐 Sécurité

L'authentification **JWT** est en place (ADR 0008) : l'identité est dérivée du jeton
(`HttpContext.User`), jamais d'un champ de requête ; les actions exigent un jeton (401 sinon),
la matrice de permissions renvoie 403, le hub SignalR est authentifié, et le **CORS est restreint**
aux origines déclarées (`Cors:Origines`). Veillez simplement à fournir une **`jwtCle` forte et secrète**
(jamais celle de développement) et un mot de passe SQL robuste.
