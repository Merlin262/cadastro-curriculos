namespace CadastroCurriculos.Application.Common;

/// <summary>
/// Shared PDF file constraints, used by both the resume-extraction validator and the
/// optional resume file accepted when creating a candidate - keeps the "PDF up to 5 MB"
/// rule defined once.
/// </summary>
public static class PdfFileRules
{
    public const int MaxFileSizeBytes = 5 * 1024 * 1024;

    private static readonly byte[] PdfMagicBytes = "%PDF"u8.ToArray();

    public static bool HasValidSize(byte[] content) => content.Length <= MaxFileSizeBytes;

    public static bool LooksLikePdf(byte[] content) =>
        content.Length >= PdfMagicBytes.Length
        && content.Take(PdfMagicBytes.Length).SequenceEqual(PdfMagicBytes);

    public static bool HasPdfExtension(string fileName) =>
        fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
}
