using FluentValidation;

namespace CadastroCurriculos.Application.Candidates.Commands.ExtractResumeData;

public sealed class ExtractResumeDataCommandValidator : AbstractValidator<ExtractResumeDataCommand>
{
    public const int MaxFileSizeBytes = 5 * 1024 * 1024;
    private static readonly byte[] PdfMagicBytes = "%PDF"u8.ToArray();

    public ExtractResumeDataCommandValidator()
    {
        RuleFor(c => c.FileName)
            .NotEmpty().WithMessage("O nome do arquivo é obrigatório.")
            .Must(name => name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Apenas arquivos PDF são aceitos.");

        RuleFor(c => c.FileContent)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("O arquivo enviado está vazio.")
            .Must(content => content.Length <= MaxFileSizeBytes)
            .WithMessage("O arquivo deve ter no máximo 5 MB.")
            .Must(LooksLikePdf)
            .WithMessage("O arquivo enviado não é um PDF válido.");
    }

    private static bool LooksLikePdf(byte[] content)
    {
        return content.Length >= PdfMagicBytes.Length
            && content.Take(PdfMagicBytes.Length).SequenceEqual(PdfMagicBytes);
    }
}
