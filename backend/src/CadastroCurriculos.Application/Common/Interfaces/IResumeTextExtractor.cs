namespace CadastroCurriculos.Application.Common.Interfaces;

public sealed record ResumeTextExtractionResult(bool Success, string Text, string? ErrorMessage);

public interface IResumeTextExtractor
{
    Task<ResumeTextExtractionResult> ExtractTextAsync(Stream pdfStream, CancellationToken cancellationToken);
}
