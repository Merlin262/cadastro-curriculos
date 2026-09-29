using CadastroCurriculos.Domain.Candidates;

namespace CadastroCurriculos.Application.Candidates.Dtos;

public sealed record CandidateListItemDto(
    Guid Id,
    string FullName,
    string Email,
    string? Phone,
    string? AreaOfInterest,
    CandidateSource Source,
    DateTime CreatedAtUtc);

public sealed record CandidateDto(
    Guid Id,
    string FullName,
    string Email,
    string? Phone,
    string? AreaOfInterest,
    string? ProfessionalSummary,
    CandidateSource Source,
    string? ResumeFileName,
    DateTime CreatedAtUtc);
