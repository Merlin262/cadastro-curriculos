using CadastroCurriculos.Application.Candidates.Queries.CheckEmailExists;
using CadastroCurriculos.Domain.Candidates;
using CadastroCurriculos.Infrastructure.Persistence.Repositories;
using CadastroCurriculos.Tests.TestSupport;

namespace CadastroCurriculos.Tests.Candidates.Queries;

public class CheckEmailExistsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsTrue_WhenEmailIsAlreadyRegistered()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var repository = new CandidateRepository(dbContext);
        dbContext.Candidates.Add(new Candidate("Ana Costa", "ana@example.com", null, null, null, CandidateSource.Manual, null));
        await dbContext.SaveChangesAsync();

        var handler = new CheckEmailExistsQueryHandler(repository);
        var result = await handler.Handle(new CheckEmailExistsQuery("ana@example.com"), CancellationToken.None);

        Assert.True(result);
    }

    [Fact]
    public async Task Handle_ReturnsFalse_WhenEmailIsNotRegistered()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var repository = new CandidateRepository(dbContext);
        var handler = new CheckEmailExistsQueryHandler(repository);

        var result = await handler.Handle(new CheckEmailExistsQuery("outro@example.com"), CancellationToken.None);

        Assert.False(result);
    }

    [Fact]
    public async Task Handle_ReturnsFalse_WithoutQueryingRepository_WhenEmailIsEmpty()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var repository = new CandidateRepository(dbContext);
        var handler = new CheckEmailExistsQueryHandler(repository);

        var result = await handler.Handle(new CheckEmailExistsQuery(""), CancellationToken.None);

        Assert.False(result);
    }
}
