using CadastroCurriculos.Application.Candidates.Parsing;

namespace CadastroCurriculos.Tests.Candidates.Parsing;

public class ResumeTextParserTests
{
    [Fact]
    public void Parse_ExtractsAllFields_FromTypicalResumeLayout()
    {
        var text = "Joao da Silva Santos\n" +
                    "joao.silva.santos@example.com\n" +
                    "(41) 99876-5432\n" +
                    "Curitiba, PR - Brasil\n" +
                    "Objetivo\n" +
                    "Atuar como Desenvolvedor Back-end .NET.";

        var result = ResumeTextParser.Parse(text);

        Assert.Equal("Joao Da Silva Santos", result.FullName);
        Assert.Equal("joao.silva.santos@example.com", result.Email);
        Assert.Equal("(41) 99876-5432", result.Phone);
    }

    [Theory]
    [InlineData("Contato: maria.oliveira@empresa.com.br", "maria.oliveira@empresa.com.br")]
    [InlineData("E-mail - joao_silva99@gmail.com;", "joao_silva99@gmail.com")]
    [InlineData("Sem nenhum email aqui", null)]
    public void ExtractEmail_FindsFirstValidEmail(string text, string? expected)
    {
        Assert.Equal(expected, ResumeTextParser.ExtractEmail(text));
    }

    [Theory]
    [InlineData("Telefone: (11) 91234-5678", "(11) 91234-5678")]
    [InlineData("Fone 41 3222-1234", "41 3222-1234")]
    [InlineData("+55 41 99876-5432", "+55 41 99876-5432")]
    [InlineData("CEP 80000-000", null)]
    public void ExtractPhone_FindsPlausiblePhoneNumber(string text, string? expected)
    {
        Assert.Equal(expected, ResumeTextParser.ExtractPhone(text));
    }

    [Fact]
    public void ExtractFullName_ReturnsNull_WhenNoLineLooksLikeAName()
    {
        var text = "CURRICULO\nresumo@example.com\n(41) 99999-8888";

        var result = ResumeTextParser.ExtractFullName(text);

        Assert.Null(result);
    }

    [Fact]
    public void ExtractFullName_SkipsSectionHeaders_AndPicksTheNameLine()
    {
        var text = "Curriculo\nAna Beatriz Costa\nObjetivo\nDesenvolvedora Frontend";

        var result = ResumeTextParser.ExtractFullName(text);

        Assert.Equal("Ana Beatriz Costa", result);
    }
}
