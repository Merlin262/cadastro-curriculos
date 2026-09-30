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
    public async Task GetPagedAsync_ReturnsEveryCandidate_MostRecentFirst()
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

        var (items, totalCount) = await repository.GetPagedAsync(null, 1, 10, CancellationToken.None);

        Assert.Equal(2, totalCount);
        Assert.Equal(2, items.Count);
        Assert.Equal(newer.Id, items[0].Id);
        Assert.Equal(older.Id, items[1].Id);
    }

    [Fact]
    public async Task GetPagedAsync_ReturnsEmpty_WhenThereAreNoCandidates()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var repository = new CandidateRepository(dbContext);

        var (items, totalCount) = await repository.GetPagedAsync(null, 1, 10, CancellationToken.None);

        Assert.Empty(items);
        Assert.Equal(0, totalCount);
    }

    [Fact]
    public async Task GetPagedAsync_FiltersByNameOrEmail()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var repository = new CandidateRepository(dbContext);

        var match = new Candidate("Ana Costa", "ana.costa@example.com", null, null, null, CandidateSource.Manual, null);
        var noMatch = new Candidate("Bruno Lima", "bruno@example.com", null, null, null, CandidateSource.Manual, null);
        await repository.AddAsync(match, CancellationToken.None);
        await repository.AddAsync(noMatch, CancellationToken.None);
        await repository.SaveChangesAsync(CancellationToken.None);

        var (items, totalCount) = await repository.GetPagedAsync("costa", 1, 10, CancellationToken.None);

        Assert.Equal(1, totalCount);
        Assert.Equal(match.Id, items[0].Id);
    }

    [Fact]
    public async Task GetPagedAsync_AppliesSkipAndTake()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var repository = new CandidateRepository(dbContext);

        for (var i = 0; i < 5; i++)
        {
            await repository.AddAsync(
                new Candidate($"Candidato {i}", $"c{i}@example.com", null, null, null, CandidateSource.Manual, null),
                CancellationToken.None);
        }

        await repository.SaveChangesAsync(CancellationToken.None);

        var (items, totalCount) = await repository.GetPagedAsync(null, page: 2, pageSize: 2, CancellationToken.None);

        Assert.Equal(5, totalCount);
        Assert.Equal(2, items.Count);
    }

    [Fact]
    public async Task EmailExistsAsync_ReturnsTrue_OnlyWhenEmailIsAlreadyRegistered()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var repository = new CandidateRepository(dbContext);
        var candidate = new Candidate("Ana Costa", "ana@example.com", null, null, null, CandidateSource.Manual, null);
        await repository.AddAsync(candidate, CancellationToken.None);
        await repository.SaveChangesAsync(CancellationToken.None);

        Assert.True(await repository.EmailExistsAsync("ana@example.com", CancellationToken.None));
        Assert.False(await repository.EmailExistsAsync("outro@example.com", CancellationToken.None));
    }
}
