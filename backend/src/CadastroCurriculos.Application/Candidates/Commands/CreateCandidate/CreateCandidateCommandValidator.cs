using FluentValidation;

namespace CadastroCurriculos.Application.Candidates.Commands.CreateCandidate;

public sealed class CreateCandidateCommandValidator : AbstractValidator<CreateCandidateCommand>
{
    public CreateCandidateCommandValidator()
    {
        RuleFor(c => c.FullName)
            .NotEmpty().WithMessage("O nome completo é obrigatório.")
            .MaximumLength(200).WithMessage("O nome completo deve ter no máximo 200 caracteres.");

        RuleFor(c => c.Email)
            .NotEmpty().WithMessage("O e-mail é obrigatório.")
            .EmailAddress().WithMessage("Informe um e-mail em um formato válido.")
            .MaximumLength(200).WithMessage("O e-mail deve ter no máximo 200 caracteres.");

        RuleFor(c => c.Phone)
            .MaximumLength(30).WithMessage("O telefone deve ter no máximo 30 caracteres.");

        RuleFor(c => c.AreaOfInterest)
            .MaximumLength(150).WithMessage("A área ou cargo de interesse deve ter no máximo 150 caracteres.");

        RuleFor(c => c.ProfessionalSummary)
            .MaximumLength(4000).WithMessage("O resumo profissional deve ter no máximo 4000 caracteres.");
    }
}
