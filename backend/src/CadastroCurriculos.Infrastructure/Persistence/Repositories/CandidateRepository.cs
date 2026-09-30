using CadastroCurriculos.Domain.Candidates;
using Microsoft.EntityFrameworkCore;

namespace CadastroCurriculos.Infrastructure.Persistence.Repositories;

public sealed class CandidateRepository : ICandidateRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CandidateRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Candidate candidate, CancellationToken cancellationToken)
    {
        await _dbContext.Candidates.AddAsync(candidate, cancellationToken);
    }

    public Task<Candidate?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext.Candidates
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<Candidate> Items, int TotalCount)> GetPagedAsync(
        string? search, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _dbContext.Candidates.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c => c.FullName.Contains(term) || c.Email.Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(c => c.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken)
    {
        var normalized = email.Trim();
        return _dbContext.Candidates
            .AsNoTracking()
            .AnyAsync(c => c.Email == normalized, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
