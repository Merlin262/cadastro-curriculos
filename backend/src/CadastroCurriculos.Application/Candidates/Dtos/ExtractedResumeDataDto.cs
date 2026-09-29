namespace CadastroCurriculos.Application.Candidates.Dtos;

public sealed record ExtractedResumeDataDto(
    bool TextExtracted,
    string? FullName,
    string? Email,
    string? Phone,
    IReadOnlyList<string> Warnings);
