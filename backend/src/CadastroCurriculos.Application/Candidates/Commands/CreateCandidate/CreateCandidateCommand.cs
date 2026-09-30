using CadastroCurriculos.Application.Candidates.Dtos;
using CadastroCurriculos.Domain.Candidates;
using LiteMediator;

namespace CadastroCurriculos.Application.Candidates.Commands.CreateCandidate;

public sealed record CreateCandidateCommand(
    string FullName,
    string Email,
    string? Phone,
    string? AreaOfInterest,
    string? ProfessionalSummary,
    CandidateSource Source,
    string? ResumeFileName,
    byte[]? ResumeFileContent) : IRequest<CandidateDto>;
