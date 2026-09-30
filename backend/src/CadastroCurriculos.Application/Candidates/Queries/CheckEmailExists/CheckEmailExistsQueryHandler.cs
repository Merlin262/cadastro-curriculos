using CadastroCurriculos.Domain.Candidates;
using LiteMediator;

namespace CadastroCurriculos.Application.Candidates.Queries.CheckEmailExists;

/// <summary>
/// Backs a soft, non-blocking duplicate-e-mail warning in the registration form: the
/// candidate can always be saved even when this returns true (see DESENVOLVIMENTO.md).
/// </summary>
public sealed class CheckEmailExistsQueryHandler : IRequestHandler<CheckEmailExistsQuery, bool>
{
    private readonly ICandidateRepository _candidateRepository;

    public CheckEmailExistsQueryHandler(ICandidateRepository candidateRepository)
    {
        _candidateRepository = candidateRepository;
    }

    public Task<bool> Handle(CheckEmailExistsQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return Task.FromResult(false);
        }

        return _candidateRepository.EmailExistsAsync(request.Email, cancellationToken);
    }
}
