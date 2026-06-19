using CVTech.SharedKernel.Domaine;

namespace CVTech.Modules.AppelOffreFreelance.Domaine;

/// <summary>Agrégat : mission ponctuelle publiée par une entreprise (publique).</summary>
public sealed class AppelOffre : RacineAgregat<Guid>
{
    public Guid EntrepriseId { get; private set; }
    public string Titre { get; private set; } = default!;
    public CahierDesCharges CahierDesCharges { get; private set; } = default!;
    public DomaineMetier Domaine { get; private set; } = default!;
    public StatutAppelOffre Statut { get; private set; }
    public Guid? PropositionLaureateId { get; private set; }
    public DateTimeOffset DatePublication { get; private set; }

    private AppelOffre() { }

    public static AppelOffre Publier(
        Guid entrepriseId, string titre, CahierDesCharges cahierDesCharges, DomaineMetier domaine)
    {
        if (entrepriseId == Guid.Empty)
            throw new ArgumentException("L'entreprise est obligatoire.", nameof(entrepriseId));
        if (string.IsNullOrWhiteSpace(titre))
            throw new ArgumentException("Le titre de l'appel d'offre est obligatoire.", nameof(titre));

        return new AppelOffre
        {
            Id = Guid.NewGuid(),
            EntrepriseId = entrepriseId,
            Titre = titre.Trim(),
            CahierDesCharges = cahierDesCharges,
            Domaine = domaine,
            Statut = StatutAppelOffre.Ouvert,
            DatePublication = DateTimeOffset.UtcNow
        };
    }

    public void SelectionnerLaureat(Guid propositionId)
    {
        if (Statut == StatutAppelOffre.Attribue)
            throw new InvalidOperationException("Cet appel d'offre est déjà attribué.");

        PropositionLaureateId = propositionId;
        Statut = StatutAppelOffre.Attribue;
    }
}
