namespace CVTech.Modules.AppelOffreFreelance.Domaine;

/// <summary>Port de persistance de l'agrégat PropositionFreelance (implémenté dans Infrastructure).</summary>
public interface IDepotPropositions
{
    Task AjouterAsync(PropositionFreelance proposition, CancellationToken ct = default);
    Task<PropositionFreelance?> ObtenirAsync(Guid id, CancellationToken ct = default);
}
