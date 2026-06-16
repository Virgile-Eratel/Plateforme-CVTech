namespace CVTech.SharedKernel.Securite;

/// <summary>
/// Plomberie transverse : fabrique un jeton JWT signé pour une identité authentifiée.
/// L'implémentation (clé de signature, émetteur) vit dans le host (Api). Le jeton porte
/// l'identifiant (claim « sub ») et le rôle, consommés ensuite par l'autorisation (ADR 0008).
/// </summary>
public interface IGenerateurJeton
{
    string Generer(Guid utilisateurId, string email, string role);
}
