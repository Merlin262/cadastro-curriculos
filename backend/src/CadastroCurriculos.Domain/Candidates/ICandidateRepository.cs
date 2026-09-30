namespace CadastroCurriculos.Domain.Candidates;

public interface ICandidateRepository
{
    Task AddAsync(Candidate candidate, CancellationToken cancellationToken);

    Task<Candidate?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Returns a page of candidates matching <paramref name="search"/> (by name or e-mail,
    /// case-insensitive), most recently registered first, along with the total match count.
    /// </summary>
    Task<(IReadOnlyList<Candidate> Items, int TotalCount)> GetPagedAsync(
        string? search, int page, int pageSize, CancellationToken cancellationToken);

    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
