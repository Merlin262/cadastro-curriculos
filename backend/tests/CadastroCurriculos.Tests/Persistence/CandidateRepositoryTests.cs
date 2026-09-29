using CadastroCurriculos.Domain.Candidates;
using CadastroCurriculos.Infrastructure.Persistence.Repositories;
using CadastroCurriculos.Tests.TestSupport;

namespace CadastroCurriculos.Tests.Persistence;

public class CandidateRepositoryTests
{
    [Fact]
    public async Task AddAsync_DoesNotPersist_UntilSaveChangesIsCalled()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var repository = new CandidateRepository(dbContext);
        var candidate = new Candidate("Ana Costa", "ana@example.com", null, null, null, CandidateSource.Manual, null);

        await repository.AddAsync(candidate, CancellationToken.None);

        Assert.Null(await repository.GetByIdAsync(candidate.Id, CancellationToken.None));

        await repository.SaveChangesAsync(CancellationToken.None);

        Assert.NotNull(await repository.GetByIdAsync(candidate.Id, CancellationToken.None));
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenCandidateDoesNotExist()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var repository = new CandidateRepository(dbContext);

        var result = await repository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEveryCandidate_MostRecentFirst()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var repository = new CandidateRepository(dbContext);

        var older = new Candidate("Ana Costa", "ana@example.com", null, null, null, CandidateSource.Manual, null);
        await repository.AddAsync(older, CancellationToken.None);
        await repository.SaveChangesAsync(CancellationToken.None);

        await Task.Delay(10);

        var newer = new Candidate("Bruno Lima", "bruno@example.com", null, null, null, CandidateSource.Pdf, "curriculo.pdf");
        await repository.AddAsync(newer, CancellationToken.None);
        await repository.SaveChangesAsync(CancellationToken.None);

        var result = await repository.GetAllAsync(CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(newer.Id, result[0].Id);
        Assert.Equal(older.Id, result[1].Id);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmpty_WhenThereAreNoCandidates()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var repository = new CandidateRepository(dbContext);

        var result = await repository.GetAllAsync(CancellationToken.None);

        Assert.Empty(result);
    }
}
