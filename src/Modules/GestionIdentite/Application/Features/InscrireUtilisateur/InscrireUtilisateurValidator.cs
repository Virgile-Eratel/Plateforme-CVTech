using FluentValidation;

namespace CVTech.Modules.GestionIdentite.Application.Features.InscrireUtilisateur;

public sealed class InscrireUtilisateurValidator : AbstractValidator<InscrireUtilisateurCommand>
{
    public InscrireUtilisateurValidator()
    {
        RuleFor(c => c.Email).NotEmpty().EmailAddress();
        RuleFor(c => c.MotDePasse).NotEmpty().MinimumLength(8)
            .WithMessage("Le mot de passe doit comporter au moins 8 caractères.");
        RuleFor(c => c.Role).IsInEnum();
    }
}
