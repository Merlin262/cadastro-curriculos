using CadastroCurriculos.Application.Candidates.Dtos;
using CadastroCurriculos.Domain.Candidates;
using LiteMediator;

namespace CadastroCurriculos.Application.Candidates.Commands.CreateCandidate;

public sealed class CreateCandidateCommandHandler : IRequestHandler<CreateCandidateCommand, CandidateDto>
{
    private readonly ICandidateRepository _candidateRepository;

    public CreateCandidateCommandHandler(ICandidateRepository candidateRepository)
    {
        _candidateRepository = candidateRepository;
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

        await _candidateRepository.AddAsync(candidate, cancellationToken);
        await _candidateRepository.SaveChangesAsync(cancellationToken);

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
