using CadastroCurriculos.Application.Candidates.Commands.CreateCandidate;
using CadastroCurriculos.Domain.Candidates;

namespace CadastroCurriculos.Tests.Candidates.Commands;

public class CreateCandidateCommandValidatorTests
{
    private readonly CreateCandidateCommandValidator _validator = new();

    private static CreateCandidateCommand ValidCommand(
        string fullName = "Maria Oliveira",
        string email = "maria@example.com",
        string? phone = "(11) 91234-5678",
        string? area = "Desenvolvimento Frontend",
        string? summary = "5 anos de experiencia.") =>
        new(fullName, email, phone, area, summary, CandidateSource.Manual, null);

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
}
