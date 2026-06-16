using CVTech.Modules.ActualiteEtAbonnement;
using CVTech.Modules.ActualiteEtAbonnement.Client;
using CVTech.Modules.AppelOffreFreelance;
using CVTech.Modules.AppelOffreFreelance.Client;
using CVTech.Modules.CatalogueEmploi;
using CVTech.Modules.CatalogueEmploi.Client;
using CVTech.Modules.GestionIdentite;
using CVTech.Modules.GestionIdentite.Client;
using System.Text;
using CVTech.Api;
using CVTech.Api.Securite;
using CVTech.SharedKernel.Comportements;
using CVTech.SharedKernel.Evenements;
using CVTech.SharedKernel.Securite;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// --- Plomberie transverse ---
// Bus d'événements interne (ADR 0003) + pipeline de validation global (une seule fois).
builder.Services.AddBusEvenements();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// --- Authentification JWT (ADR 0008) ---
builder.Services.AddSingleton<IGenerateurJeton, GenerateurJetonJwt>();
var jwt = ParametresJwt.Lire(builder.Configuration);
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Emetteur,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Cle))
        };

        // SignalR (WASM) transmet le jeton via la query string sur la route du hub.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = contexte =>
            {
                var jetonAcces = contexte.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(jetonAcces) &&
                    contexte.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                {
                    contexte.Token = jetonAcces;
                }
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

// CORS restreint aux origines déclarées ("Cors:Origines"). Le front hébergé par l'API est
// en même origine (CORS inutile) ; la liste sert aux fronts servis ailleurs (dev, démo).
var originesAutorisees = builder.Configuration.GetSection("Cors:Origines").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddDefaultPolicy(p =>
{
    if (originesAutorisees.Length > 0)
        p.WithOrigins(originesAutorisees).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
}));

// --- Persistance (ADR 0005) : SQLite en local/dev (un fichier par module), Azure SQL au déploiement.
// Le provider est choisi par configuration ("Persistence:Provider" = "Sqlite" | "SqlServer").
var fournisseur = builder.Configuration["Persistence:Provider"] ?? "Sqlite";
var chaineConnexion = builder.Configuration.GetConnectionString("CVTech");

Action<DbContextOptionsBuilder> Bdd(string schema) => options =>
{
    if (fournisseur.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
        options.UseSqlServer(chaineConnexion,
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", schema));
    else
        options.UseSqlite($"Data Source=cvtech-{schema}.db");
};

// --- Enregistrement des 4 modules (composition root global) ---
builder.Services.AddModuleGestionIdentite(Bdd("identite"));
builder.Services.AddModuleCatalogueEmploi(Bdd("emploi"));
builder.Services.AddModuleAppelOffreFreelance(Bdd("freelance"));
builder.Services.AddModuleActualiteEtAbonnement(Bdd("actualite"));

var app = builder.Build();

// Prépare la base au démarrage (ignoré par l'outillage EF design-time : migrations add/script).
if (!EF.IsDesignTime)
    await app.InitialiserBaseDeDonneesAsync(fournisseur);

app.UseCors();

// Sert le front Blazor WebAssembly hébergé dans le même App Service (ADR 0007/0010).
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

// Authentification/autorisation (ADR 0008) — avant les endpoints.
app.UseAuthentication();
app.UseAuthorization();

// --- Endpoints exposés par chaque module (couche Client) ---
app.MapGestionIdentite();
app.MapCatalogueEmploi();
app.MapAppelOffreFreelance();
app.MapActualiteEtAbonnement();

// Toute route non-API renvoie l'app Blazor (routage côté client).
app.MapFallbackToFile("index.html");

app.Run();

// Rend la classe Program accessible aux tests d'intégration éventuels.
public partial class Program;
