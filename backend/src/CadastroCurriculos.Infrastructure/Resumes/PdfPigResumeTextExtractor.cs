using System.Text;
using CadastroCurriculos.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace CadastroCurriculos.Infrastructure.Resumes;

public sealed class PdfPigResumeTextExtractor : IResumeTextExtractor
{
    // Words whose vertical position differs by less than this (in PDF points) are
    // considered part of the same visual line.
    private const double SameLineTolerance = 3.0;

    private readonly ILogger<PdfPigResumeTextExtractor> _logger;

    public PdfPigResumeTextExtractor(ILogger<PdfPigResumeTextExtractor> logger)
    {
        _logger = logger;
    }

    public Task<ResumeTextExtractionResult> ExtractTextAsync(Stream pdfStream, CancellationToken cancellationToken)
    {
        try
        {
            using var document = PdfDocument.Open(pdfStream);
            var builder = new StringBuilder();

            foreach (var page in document.GetPages())
            {
                cancellationToken.ThrowIfCancellationRequested();
                builder.AppendLine(ExtractPageTextByLine(page));
            }

            return Task.FromResult(new ResumeTextExtractionResult(true, builder.ToString(), null));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            // A corrupted, encrypted or otherwise unreadable PDF must not block the candidate
            // from registering manually - we surface a friendly message instead of throwing.
            _logger.LogWarning(ex, "Falha ao extrair texto do PDF enviado.");
            return Task.FromResult(new ResumeTextExtractionResult(
                false,
                string.Empty,
                "Não foi possível ler o conteúdo do PDF. O arquivo pode estar corrompido, protegido ou ser uma imagem digitalizada."));
        }
    }

    /// <summary>
    /// PdfPig's <see cref="Page.Text"/> concatenates every word on the page with no
    /// regard for line breaks, which merges the end of one line into the start of the
    /// next (e.g. a name immediately followed by an e-mail address). Rebuilding lines
    /// from word bounding boxes keeps the document's visual layout, which the resume
    /// field heuristics in <c>ResumeTextParser</c> rely on.
    /// </summary>
    private static string ExtractPageTextByLine(Page page)
    {
        var wordsTopToBottom = page.GetWords()
            .OrderByDescending(w => w.BoundingBox.Top)
            .ToList();

        var lines = new List<List<Word>>();
        List<Word>? currentLine = null;
        double? currentLineTop = null;

        foreach (var word in wordsTopToBottom)
        {
            if (currentLine is not null && currentLineTop is not null &&
                Math.Abs(currentLineTop.Value - word.BoundingBox.Top) <= SameLineTolerance)
            {
                currentLine.Add(word);
            }
            else
            {
                currentLine = new List<Word> { word };
                lines.Add(currentLine);
                currentLineTop = word.BoundingBox.Top;
            }
        }

        var lineTexts = lines.Select(line =>
            string.Join(' ', line.OrderBy(w => w.BoundingBox.Left).Select(w => w.Text)));

        return string.Join('\n', lineTexts);
    }
}
