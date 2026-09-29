using CadastroCurriculos.Application.Candidates.Dtos;
using CadastroCurriculos.Application.Common.Interfaces;
using LiteMediator;
using Microsoft.EntityFrameworkCore;

namespace CadastroCurriculos.Application.Candidates.Queries.GetCandidateById;

public sealed class GetCandidateByIdQueryHandler : IRequestHandler<GetCandidateByIdQuery, CandidateDto?>
{
    private readonly IApplicationDbContext _dbContext;

    public GetCandidateByIdQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CandidateDto?> Handle(GetCandidateByIdQuery request, CancellationToken cancellationToken)
    {
        var candidate = await _dbContext.Candidates
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

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
            candidate.CreatedAtUtc);
    }
}
