using CadastroCurriculos.Application.Common;
using FluentValidation;

namespace CadastroCurriculos.Application.Candidates.Commands.ExtractResumeData;

public sealed class ExtractResumeDataCommandValidator : AbstractValidator<ExtractResumeDataCommand>
{
    public ExtractResumeDataCommandValidator()
    {
        RuleFor(c => c.FileName)
            .NotEmpty().WithMessage("O nome do arquivo é obrigatório.")
            .Must(PdfFileRules.HasPdfExtension)
            .WithMessage("Apenas arquivos PDF são aceitos.");

        RuleFor(c => c.FileContent)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("O arquivo enviado está vazio.")
            .Must(PdfFileRules.HasValidSize)
            .WithMessage("O arquivo deve ter no máximo 5 MB.")
            .Must(PdfFileRules.LooksLikePdf)
            .WithMessage("O arquivo enviado não é um PDF válido.");
    }
}
