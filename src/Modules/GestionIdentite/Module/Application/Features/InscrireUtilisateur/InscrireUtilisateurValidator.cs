using FluentValidation;

namespace CVTech.Modules.GestionIdentite.Application.Features.InscrireUtilisateur;

public sealed class InscrireUtilisateurValidator : AbstractValidator<InscrireUtilisateurCommand>
{
    public InscrireUtilisateurValidator()
    {
        RuleFor(c => c.Email).NotEmpty().EmailAddress();
        RuleFor(c => c.Role).IsInEnum();
    }
}
