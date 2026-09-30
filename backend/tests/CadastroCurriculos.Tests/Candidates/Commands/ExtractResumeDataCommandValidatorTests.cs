using System.Text;
using CadastroCurriculos.Application.Candidates.Commands.ExtractResumeData;
using CadastroCurriculos.Application.Common;

namespace CadastroCurriculos.Tests.Candidates.Commands;

public class ExtractResumeDataCommandValidatorTests
{
    private readonly ExtractResumeDataCommandValidator _validator = new();

    private static byte[] FakePdfBytes(int totalSize = 100)
    {
        var content = Encoding.ASCII.GetBytes("%PDF-1.7\n%fake pdf content for tests");
        if (totalSize <= content.Length)
        {
            return content[..totalSize];
        }

        var padded = new byte[totalSize];
        Array.Copy(content, padded, content.Length);
        return padded;
    }

    [Fact]
    public void Validate_Succeeds_ForValidPdfWithinSizeLimit()
    {
        var command = new ExtractResumeDataCommand(FakePdfBytes(), "curriculo.pdf");

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Fails_WhenFileExceedsFiveMegabytes()
    {
        var oversized = FakePdfBytes(PdfFileRules.MaxFileSizeBytes + 1);
        var command = new ExtractResumeDataCommand(oversized, "curriculo.pdf");

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("5 MB"));
    }

    [Fact]
    public void Validate_Fails_WhenExtensionIsNotPdf()
    {
        var command = new ExtractResumeDataCommand(FakePdfBytes(), "curriculo.docx");

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_Fails_WhenContentIsNotActuallyAPdf()
    {
        var notPdf = Encoding.ASCII.GetBytes("isso nao e um pdf de verdade");
        var command = new ExtractResumeDataCommand(notPdf, "curriculo.pdf");

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("não é um PDF válido"));
    }

    [Fact]
    public void Validate_Fails_WhenFileIsEmpty()
    {
        var command = new ExtractResumeDataCommand([], "curriculo.pdf");

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }
}
