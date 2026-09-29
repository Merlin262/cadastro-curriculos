using CadastroCurriculos.Application.Candidates.Dtos;
using CadastroCurriculos.Domain.Candidates;
using LiteMediator;

namespace CadastroCurriculos.Application.Candidates.Queries.GetCandidates;

public sealed class GetCandidatesQueryHandler : IRequestHandler<GetCandidatesQuery, List<CandidateListItemDto>>
{
    private readonly ICandidateRepository _candidateRepository;

    public GetCandidatesQueryHandler(ICandidateRepository candidateRepository)
    {
        _candidateRepository = candidateRepository;
    }

    public async Task<List<CandidateListItemDto>> Handle(GetCandidatesQuery request, CancellationToken cancellationToken)
    {
        var candidates = await _candidateRepository.GetAllAsync(cancellationToken);

        return candidates
            .Select(c => new CandidateListItemDto(
                c.Id,
                c.FullName,
                c.Email,
                c.Phone,
                c.AreaOfInterest,
                c.Source,
                c.CreatedAtUtc))
            .ToList();
    }
}
