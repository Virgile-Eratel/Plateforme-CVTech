using CVTech.Modules.AppelOffreFreelance.Domaine;
using CVTech.SharedKernel.Domaine;
using FluentAssertions;

namespace CVTech.Modules.AppelOffreFreelance.Tests;

public class DomaineTests
{
    [Fact]
    public void UnTjmNegatifOuNulEstRefuse()
    {
        Action creation = () => BaremeTJM.Creer(0m);

        creation.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UnAppelOffreNePeutPasEtreAttribueDeuxFois()
    {
        var ao = AppelOffre.Publier(Guid.NewGuid(), "Mission",
            CahierDesCharges.Creer("c", "l", DateTimeOffset.UtcNow.AddDays(10), 0m, 100m),
            DomaineMetier.Creer("DevOps"));
        ao.SelectionnerLaureat(Guid.NewGuid());

        Action secondeSelection = () => ao.SelectionnerLaureat(Guid.NewGuid());

        secondeSelection.Should().Throw<InvalidOperationException>();
        ao.Statut.Should().Be(StatutAppelOffre.Attribue);
    }
}
