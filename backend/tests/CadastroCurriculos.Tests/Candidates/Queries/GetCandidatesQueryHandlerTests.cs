using CadastroCurriculos.Application.Candidates.Queries.GetCandidateById;
using CadastroCurriculos.Application.Candidates.Queries.GetCandidates;
using CadastroCurriculos.Domain.Candidates;
using CadastroCurriculos.Infrastructure.Persistence.Repositories;
using CadastroCurriculos.Tests.TestSupport;

namespace CadastroCurriculos.Tests.Candidates.Queries;

public class GetCandidatesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsCandidates_MostRecentFirst()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var repository = new CandidateRepository(dbContext);

        var older = new Candidate("Ana Costa", "ana@example.com", null, null, null, CandidateSource.Manual, null);
        await Task.Delay(10);
        var newer = new Candidate("Bruno Lima", "bruno@example.com", null, null, null, CandidateSource.Pdf, "curriculo.pdf");

        dbContext.Candidates.AddRange(older, newer);
        await dbContext.SaveChangesAsync();

        var handler = new GetCandidatesQueryHandler(repository);
        var result = await handler.Handle(new GetCandidatesQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(newer.Id, result[0].Id);
        Assert.Equal(older.Id, result[1].Id);
    }

    [Fact]
    public async Task Handle_ReturnsEmptyList_WhenNoCandidatesExist()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var repository = new CandidateRepository(dbContext);
        var handler = new GetCandidatesQueryHandler(repository);

        var result = await handler.Handle(new GetCandidatesQuery(), CancellationToken.None);

        Assert.Empty(result);
    }
}

public class GetCandidateByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsNull_WhenCandidateDoesNotExist()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var repository = new CandidateRepository(dbContext);
        var handler = new GetCandidateByIdQueryHandler(repository);

        var result = await handler.Handle(new GetCandidateByIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_ReturnsFullDetails_WhenCandidateExists()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var repository = new CandidateRepository(dbContext);
        var candidate = new Candidate(
            "Ana Costa", "ana@example.com", "(41) 90000-0000", "Dados", "Resumo profissional", CandidateSource.Manual, null);
        dbContext.Candidates.Add(candidate);
        await dbContext.SaveChangesAsync();

        var handler = new GetCandidateByIdQueryHandler(repository);
        var result = await handler.Handle(new GetCandidateByIdQuery(candidate.Id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Ana Costa", result!.FullName);
        Assert.Equal("Resumo profissional", result.ProfessionalSummary);
    }
}
