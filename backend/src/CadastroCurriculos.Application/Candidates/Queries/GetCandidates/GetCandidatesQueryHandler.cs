using CadastroCurriculos.Application.Candidates.Dtos;
using CadastroCurriculos.Application.Common.Interfaces;
using LiteMediator;
using Microsoft.EntityFrameworkCore;

namespace CadastroCurriculos.Application.Candidates.Queries.GetCandidates;

public sealed class GetCandidatesQueryHandler : IRequestHandler<GetCandidatesQuery, List<CandidateListItemDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetCandidatesQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<CandidateListItemDto>> Handle(GetCandidatesQuery request, CancellationToken cancellationToken)
    {
        return await _dbContext.Candidates
            .OrderByDescending(c => c.CreatedAtUtc)
            .Select(c => new CandidateListItemDto(
                c.Id,
                c.FullName,
                c.Email,
                c.Phone,
                c.AreaOfInterest,
                c.Source,
                c.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }
}
