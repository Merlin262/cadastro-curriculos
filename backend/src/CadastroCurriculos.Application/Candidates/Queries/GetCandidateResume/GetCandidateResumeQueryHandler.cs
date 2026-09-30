using CadastroCurriculos.Application.Candidates.Dtos;
using CadastroCurriculos.Domain.Candidates;
using LiteMediator;

namespace CadastroCurriculos.Application.Candidates.Queries.GetCandidateResume;

public sealed class GetCandidateResumeQueryHandler : IRequestHandler<GetCandidateResumeQuery, CandidateResumeDto?>
{
    private readonly ICandidateRepository _candidateRepository;

    public GetCandidateResumeQueryHandler(ICandidateRepository candidateRepository)
    {
        _candidateRepository = candidateRepository;
    }

    public async Task<CandidateResumeDto?> Handle(GetCandidateResumeQuery request, CancellationToken cancellationToken)
    {
        var candidate = await _candidateRepository.GetByIdAsync(request.Id, cancellationToken);

        if (candidate?.ResumeFileContent is null)
        {
            return null;
        }

        return new CandidateResumeDto(candidate.ResumeFileContent, candidate.ResumeFileName ?? "curriculo.pdf");
    }
}
