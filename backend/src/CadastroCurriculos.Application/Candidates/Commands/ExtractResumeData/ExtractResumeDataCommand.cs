using CadastroCurriculos.Application.Candidates.Dtos;
using LiteMediator;

namespace CadastroCurriculos.Application.Candidates.Commands.ExtractResumeData;

public sealed record ExtractResumeDataCommand(
    byte[] FileContent,
    string FileName) : IRequest<ExtractedResumeDataDto>;
