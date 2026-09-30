using CadastroCurriculos.Application.Candidates.Dtos;
using CadastroCurriculos.Domain.Candidates;
using LiteMediator;

namespace CadastroCurriculos.Application.Candidates.Queries.GetCandidateById;

public sealed class GetCandidateByIdQueryHandler : IRequestHandler<GetCandidateByIdQuery, CandidateDto?>
{
    private readonly ICandidateRepository _candidateRepository;

    public GetCandidateByIdQueryHandler(ICandidateRepository candidateRepository)
    {
        _candidateRepository = candidateRepository;
    }

    public async Task<CandidateDto?> Handle(GetCandidateByIdQuery request, CancellationToken cancellationToken)
    {
        var candidate = await _candidateRepository.GetByIdAsync(request.Id, cancellationToken);

        if (candidate is null)
        {
            return null;
        }

        return new CandidateDto(
            candidate.Id,
            candidate.FullName,
            candidate.Email,
            candidate.Phone,
            candidate.AreaOfInterest,
            candidate.ProfessionalSummary,
            candidate.Source,
            candidate.ResumeFileName,
            candidate.HasResumeFile,
            candidate.CreatedAtUtc);
    }
}
