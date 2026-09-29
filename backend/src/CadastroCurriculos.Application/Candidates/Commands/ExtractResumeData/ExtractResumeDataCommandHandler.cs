using CadastroCurriculos.Application.Candidates.Dtos;
using CadastroCurriculos.Application.Candidates.Parsing;
using CadastroCurriculos.Application.Common.Interfaces;
using LiteMediator;

namespace CadastroCurriculos.Application.Candidates.Commands.ExtractResumeData;

public sealed class ExtractResumeDataCommandHandler : IRequestHandler<ExtractResumeDataCommand, ExtractedResumeDataDto>
{
    private readonly IResumeTextExtractor _textExtractor;

    public ExtractResumeDataCommandHandler(IResumeTextExtractor textExtractor)
    {
        _textExtractor = textExtractor;
    }

    public async Task<ExtractedResumeDataDto> Handle(ExtractResumeDataCommand request, CancellationToken cancellationToken)
    {
        using var stream = new MemoryStream(request.FileContent);
        var extraction = await _textExtractor.ExtractTextAsync(stream, cancellationToken);

        var warnings = new List<string>();

        if (!extraction.Success)
        {
            warnings.Add(extraction.ErrorMessage
                ?? "Não foi possível ler o conteúdo do PDF. Preencha os campos manualmente.");
            return new ExtractedResumeDataDto(false, null, null, null, warnings);
        }

        if (string.IsNullOrWhiteSpace(extraction.Text))
        {
            warnings.Add("O PDF não contém texto reconhecível (pode ser um documento digitalizado/escaneado). Preencha os campos manualmente.");
            return new ExtractedResumeDataDto(false, null, null, null, warnings);
        }

        var fields = ResumeTextParser.Parse(extraction.Text);

        if (fields.FullName is null)
        {
            warnings.Add("Não foi possível identificar o nome automaticamente. Preencha manualmente.");
        }

        if (fields.Email is null)
        {
            warnings.Add("Não foi possível identificar o e-mail automaticamente. Preencha manualmente.");
        }

        if (fields.Phone is null)
        {
            warnings.Add("Não foi possível identificar o telefone automaticamente. Preencha manualmente, se desejar.");
        }

        return new ExtractedResumeDataDto(true, fields.FullName, fields.Email, fields.Phone, warnings);
    }
}
