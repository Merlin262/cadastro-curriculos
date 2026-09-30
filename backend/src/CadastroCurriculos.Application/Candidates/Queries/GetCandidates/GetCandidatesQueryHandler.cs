using CadastroCurriculos.Application.Candidates.Dtos;
using CadastroCurriculos.Application.Common;
using CadastroCurriculos.Domain.Candidates;
using LiteMediator;

namespace CadastroCurriculos.Application.Candidates.Queries.GetCandidates;

public sealed class GetCandidatesQueryHandler : IRequestHandler<GetCandidatesQuery, PagedResult<CandidateListItemDto>>
{
    private readonly ICandidateRepository _candidateRepository;

    public GetCandidatesQueryHandler(ICandidateRepository candidateRepository)
    {
        _candidateRepository = candidateRepository;
    }

    public async Task<PagedResult<CandidateListItemDto>> Handle(GetCandidatesQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _candidateRepository.GetPagedAsync(
            request.Search, request.Page, request.PageSize, cancellationToken);

        var dtos = items
            .Select(c => new CandidateListItemDto(
                c.Id,
                c.FullName,
                c.Email,
                c.Phone,
                c.AreaOfInterest,
                c.Source,
                c.CreatedAtUtc))
            .ToList();

        return new PagedResult<CandidateListItemDto>(dtos, totalCount, request.Page, request.PageSize);
    }
}
