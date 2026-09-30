using System.Text;
using CadastroCurriculos.Application.Candidates.Commands.CreateCandidate;
using CadastroCurriculos.Application.Common;
using CadastroCurriculos.Domain.Candidates;

namespace CadastroCurriculos.Tests.Candidates.Commands;

public class CreateCandidateCommandValidatorTests
{
    private readonly CreateCandidateCommandValidator _validator = new();

    private static readonly byte[] ValidPdfBytes = Encoding.ASCII.GetBytes("%PDF-1.7\n%conteudo de teste");

    private static CreateCandidateCommand ValidCommand(
        string fullName = "Maria Oliveira",
        string email = "maria@example.com",
        string? phone = "(11) 91234-5678",
        string? area = "Desenvolvimento Frontend",
        string? summary = "5 anos de experiencia.",
        string? resumeFileName = null,
        byte[]? resumeFileContent = null) =>
        new(fullName, email, phone, area, summary, CandidateSource.Manual, resumeFileName, resumeFileContent);

    [Fact]
    public void Validate_Succeeds_ForCompleteValidCommand()
    {
        var result = _validator.Validate(ValidCommand());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Succeeds_WhenOptionalFieldsAreMissing()
    {
        var result = _validator.Validate(ValidCommand(phone: null, area: null, summary: null));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_Fails_WhenFullNameIsMissing(string fullName)
    {
        var result = _validator.Validate(ValidCommand(fullName: fullName));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCandidateCommand.FullName));
    }

    [Theory]
    [InlineData("")]
    [InlineData("nao-e-um-email")]
    [InlineData("sem-arroba.com")]
    public void Validate_Fails_ForMissingOrInvalidEmail(string email)
    {
        var result = _validator.Validate(ValidCommand(email: email));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCandidateCommand.Email));
    }

    [Fact]
    public void Validate_Succeeds_WithAValidAttachedResume()
    {
        var result = _validator.Validate(ValidCommand(resumeFileName: "curriculo.pdf", resumeFileContent: ValidPdfBytes));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Fails_WhenAttachedResumeExceedsFiveMegabytes()
    {
        var oversized = new byte[PdfFileRules.MaxFileSizeBytes + 1];
        Array.Copy(ValidPdfBytes, oversized, ValidPdfBytes.Length);

        var result = _validator.Validate(ValidCommand(resumeFileName: "curriculo.pdf", resumeFileContent: oversized));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("5 MB"));
    }

    [Fact]
    public void Validate_Fails_WhenAttachedResumeContentIsNotActuallyAPdf()
    {
        var notPdf = Encoding.ASCII.GetBytes("isso nao e um pdf de verdade");

        var result = _validator.Validate(ValidCommand(resumeFileName: "curriculo.pdf", resumeFileContent: notPdf));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("não é um PDF válido"));
    }

    [Fact]
    public void Validate_Fails_WhenAttachedResumeHasWrongExtension()
    {
        var result = _validator.Validate(ValidCommand(resumeFileName: "curriculo.docx", resumeFileContent: ValidPdfBytes));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("PDF"));
    }
}
