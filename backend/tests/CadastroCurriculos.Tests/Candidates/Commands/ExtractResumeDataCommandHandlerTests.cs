using CadastroCurriculos.Application.Candidates.Commands.ExtractResumeData;
using CadastroCurriculos.Application.Common.Interfaces;

namespace CadastroCurriculos.Tests.Candidates.Commands;

public class ExtractResumeDataCommandHandlerTests
{
    private sealed class StubTextExtractor : IResumeTextExtractor
    {
        private readonly ResumeTextExtractionResult _result;

        public StubTextExtractor(ResumeTextExtractionResult result) => _result = result;

        public Task<ResumeTextExtractionResult> ExtractTextAsync(Stream pdfStream, CancellationToken cancellationToken) =>
            Task.FromResult(_result);
    }

    [Fact]
    public async Task Handle_ReturnsParsedFields_WhenTextExtractionSucceeds()
    {
        var text = "Joao Silva\njoao@example.com\n(41) 99876-5432";
        var handler = new ExtractResumeDataCommandHandler(
            new StubTextExtractor(new ResumeTextExtractionResult(true, text, null)));

        var result = await handler.Handle(new ExtractResumeDataCommand([1, 2, 3], "curriculo.pdf"), CancellationToken.None);

        Assert.True(result.TextExtracted);
        Assert.Equal("Joao Silva", result.FullName);
        Assert.Equal("joao@example.com", result.Email);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public async Task Handle_ReturnsWarningsWithoutThrowing_WhenPdfCannotBeRead()
    {
        var handler = new ExtractResumeDataCommandHandler(
            new StubTextExtractor(new ResumeTextExtractionResult(false, string.Empty, "arquivo corrompido")));

        var result = await handler.Handle(new ExtractResumeDataCommand([1, 2, 3], "curriculo.pdf"), CancellationToken.None);

        Assert.False(result.TextExtracted);
        Assert.Null(result.FullName);
        Assert.Null(result.Email);
        Assert.Null(result.Phone);
        Assert.Contains("arquivo corrompido", result.Warnings);
    }

    [Fact]
    public async Task Handle_WarnsPerMissingField_WhenOnlySomeFieldsAreFound()
    {
        var text = "Nenhum dado reconhecivel aqui, apenas texto solto sem estrutura.";
        var handler = new ExtractResumeDataCommandHandler(
            new StubTextExtractor(new ResumeTextExtractionResult(true, text, null)));

        var result = await handler.Handle(new ExtractResumeDataCommand([1, 2, 3], "curriculo.pdf"), CancellationToken.None);

        Assert.True(result.TextExtracted);
        Assert.Null(result.FullName);
        Assert.Null(result.Email);
        Assert.Null(result.Phone);
        Assert.Equal(3, result.Warnings.Count);
    }
}
