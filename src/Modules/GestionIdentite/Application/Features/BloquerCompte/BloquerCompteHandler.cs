using CVTech.Modules.GestionIdentite.Application;
using CVTech.Modules.GestionIdentite.Contracts;
using MediatR;

using CVTech.Modules.GestionIdentite.Domaine;

namespace CVTech.Modules.GestionIdentite.Application.Features.BloquerCompte;

public sealed class BloquerCompteHandler(
    IVerificateurPermission permissions,
    IDepotUtilisateurs depot) : IRequestHandler<BloquerCompteCommand>
{
    public async Task Handle(BloquerCompteCommand commande, CancellationToken ct)
    {
        // 1. PERMISSION D'ABORD — toujours en première ligne (ADR 0004, skill cvtech-permissions).
        await permissions.ExigerAsync(commande.AppelantId, ActionMetier.BloquerOuReactiverCompte, ct: ct);

        // 2. Action métier.
        var cible = await depot.ObtenirAsync(commande.CompteCibleId, ct)
            ?? throw new InvalidOperationException("Compte cible introuvable.");

        cible.Bloquer();
        await depot.MettreAJourAsync(cible, ct);
    }
}
