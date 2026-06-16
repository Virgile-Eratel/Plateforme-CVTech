using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CVTech.SharedKernel.Securite;
using Microsoft.IdentityModel.Tokens;

namespace CVTech.Api.Securite;

/// <summary>
/// Fabrique des jetons JWT signés (HMAC-SHA256). La clé et l'émetteur proviennent de la
/// configuration (« Jwt:* ») — en production via une variable secrète de pipeline (ADR 0010).
/// </summary>
public sealed class GenerateurJetonJwt(IConfiguration configuration) : IGenerateurJeton
{
    public string Generer(Guid utilisateurId, string email, string role)
    {
        var parametres = ParametresJwt.Lire(configuration);
        var cle = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(parametres.Cle));
        var identifiants = new SigningCredentials(cle, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, utilisateurId.ToString()),
            new Claim(ClaimTypes.NameIdentifier, utilisateurId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.Role, role),
            new Claim("role", role)
        };

        var jeton = new JwtSecurityToken(
            issuer: parametres.Emetteur,
            audience: parametres.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(parametres.DureeMinutes),
            signingCredentials: identifiants);

        return new JwtSecurityTokenHandler().WriteToken(jeton);
    }
}

/// <summary>Paramètres de signature/validation JWT, lus depuis la configuration.</summary>
public sealed record ParametresJwt(string Cle, string Emetteur, string Audience, int DureeMinutes)
{
    public static ParametresJwt Lire(IConfiguration configuration)
    {
        var section = configuration.GetSection("Jwt");
        var cle = section["Cle"];
        if (string.IsNullOrWhiteSpace(cle) || cle.Length < 32)
            throw new InvalidOperationException(
                "Configuration 'Jwt:Cle' absente ou trop courte (>= 32 caractères requis).");

        return new ParametresJwt(
            Cle: cle,
            Emetteur: section["Emetteur"] ?? "cvtech",
            Audience: section["Audience"] ?? "cvtech",
            DureeMinutes: int.TryParse(section["DureeMinutes"], out var d) ? d : 120);
    }
}
