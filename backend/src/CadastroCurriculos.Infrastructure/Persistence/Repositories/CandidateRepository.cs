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

    public async Task<IReadOnlyList<Candidate>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Candidates
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
