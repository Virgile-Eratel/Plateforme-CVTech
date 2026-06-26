using CVTech.Modules.CatalogueEmploi.Domaine;
using FluentAssertions;

namespace CVTech.Modules.CatalogueEmploi.Tests;

public class CurriculumVitaeTests
{
    [Fact]
    public void UnCvSansPresentationEstRefuse()
    {
        Action action = () => CurriculumVitae.Constituer(Guid.NewGuid(), "   ", new[] { "C#" });

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void MettreAJourRemplaceLaPresentationEtLesCompetences()
    {
        var cv = CurriculumVitae.Constituer(Guid.NewGuid(), "Présentation initiale", new[] { "C#" });

        cv.MettreAJour("Nouvelle présentation", new[] { "Azure", "Terraform" });

        cv.Presentation.Should().Be("Nouvelle présentation");
        cv.Competences.Should().BeEquivalentTo("Azure", "Terraform");
    }

    [Fact]
    public void MettreAJourAvecUnePresentationVideEstRefuse()
    {
        var cv = CurriculumVitae.Constituer(Guid.NewGuid(), "Présentation initiale", new[] { "C#" });

        Action action = () => cv.MettreAJour("  ", new[] { "Azure" });

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UnAgeRenseigneEstConserveSurLeCv()
    {
        var cv = CurriculumVitae.Constituer(Guid.NewGuid(), "Présentation", new[] { "C#" }, age: 34);

        cv.Age.Should().Be(34);
    }

    [Fact]
    public void UnCvSansAgeRenseigneLaisseLAgeNonDefini()
    {
        var cv = CurriculumVitae.Constituer(Guid.NewGuid(), "Présentation", new[] { "C#" });

        cv.Age.Should().BeNull();
    }

    [Theory]
    [InlineData(15)]
    [InlineData(0)]
    [InlineData(-3)]
    [InlineData(101)]
    public void UnAgeHorsBornesDeTravailEstRefuse(int ageInvalide)
    {
        Action action = () =>
            CurriculumVitae.Constituer(Guid.NewGuid(), "Présentation", new[] { "C#" }, age: ageInvalide);

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void MettreAJourRevisLAgeDuCv()
    {
        var cv = CurriculumVitae.Constituer(Guid.NewGuid(), "Présentation", new[] { "C#" }, age: 30);

        cv.MettreAJour("Présentation", new[] { "C#" }, age: 31);

        cv.Age.Should().Be(31);
    }
}
