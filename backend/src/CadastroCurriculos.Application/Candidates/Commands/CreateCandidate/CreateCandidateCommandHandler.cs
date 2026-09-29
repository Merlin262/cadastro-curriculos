using CadastroCurriculos.Application.Candidates.Dtos;
using CadastroCurriculos.Application.Common.Interfaces;
using CadastroCurriculos.Domain.Candidates;
using LiteMediator;

namespace CadastroCurriculos.Application.Candidates.Commands.CreateCandidate;

public sealed class CreateCandidateCommandHandler : IRequestHandler<CreateCandidateCommand, CandidateDto>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateCandidateCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CandidateDto> Handle(CreateCandidateCommand request, CancellationToken cancellationToken)
    {
        var candidate = new Candidate(
            request.FullName,
            request.Email,
            request.Phone,
            request.AreaOfInterest,
            request.ProfessionalSummary,
            request.Source,
            request.ResumeFileName);

        _dbContext.Candidates.Add(candidate);
        await _dbContext.SaveChangesAsync(cancellationToken);

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
