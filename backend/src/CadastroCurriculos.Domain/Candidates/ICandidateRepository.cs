namespace CadastroCurriculos.Domain.Candidates;

public interface ICandidateRepository
{
    Task AddAsync(Candidate candidate, CancellationToken cancellationToken);

    Task<Candidate?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Returns every candidate, most recently registered first.</summary>
    Task<IReadOnlyList<Candidate>> GetAllAsync(CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
