using CadastroCurriculos.Application.Common;
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

        // The resume file is optional here too: it was already validated once at extraction
        // time, but we re-check it in case the same file is (re-)attached on save.
        When(c => c.ResumeFileContent is not null, () =>
        {
            RuleFor(c => c.ResumeFileContent!)
                .Must(PdfFileRules.HasValidSize).WithMessage("O arquivo deve ter no máximo 5 MB.")
                .Must(PdfFileRules.LooksLikePdf).WithMessage("O arquivo enviado não é um PDF válido.");

            RuleFor(c => c.ResumeFileName)
                .NotEmpty().WithMessage("O nome do arquivo é obrigatório.")
                .Must(name => name is not null && PdfFileRules.HasPdfExtension(name))
                .WithMessage("Apenas arquivos PDF são aceitos.");
        });
    }
}
