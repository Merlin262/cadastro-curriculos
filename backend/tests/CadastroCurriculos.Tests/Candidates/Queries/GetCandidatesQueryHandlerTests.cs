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

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(newer.Id, result.Items[0].Id);
        Assert.Equal(older.Id, result.Items[1].Id);
    }

    [Fact]
    public async Task Handle_ReturnsEmptyResult_WhenNoCandidatesExist()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var repository = new CandidateRepository(dbContext);
        var handler = new GetCandidatesQueryHandler(repository);

        var result = await handler.Handle(new GetCandidatesQuery(), CancellationToken.None);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task Handle_FiltersByNameOrEmail_WhenSearchIsProvided()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var repository = new CandidateRepository(dbContext);

        var match = new Candidate("Ana Costa", "ana.costa@example.com", null, null, null, CandidateSource.Manual, null);
        var noMatch = new Candidate("Bruno Lima", "bruno@example.com", null, null, null, CandidateSource.Manual, null);
        dbContext.Candidates.AddRange(match, noMatch);
        await dbContext.SaveChangesAsync();

        var handler = new GetCandidatesQueryHandler(repository);
        var result = await handler.Handle(new GetCandidatesQuery(Search: "ana"), CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal(match.Id, result.Items[0].Id);
    }

    [Fact]
    public async Task Handle_RespectsPageAndPageSize()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var repository = new CandidateRepository(dbContext);

        for (var i = 0; i < 5; i++)
        {
            dbContext.Candidates.Add(new Candidate($"Candidato {i}", $"c{i}@example.com", null, null, null, CandidateSource.Manual, null));
        }

        await dbContext.SaveChangesAsync();

        var handler = new GetCandidatesQueryHandler(repository);
        var result = await handler.Handle(new GetCandidatesQuery(Page: 2, PageSize: 2), CancellationToken.None);

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
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
        Assert.False(result.HasResumeFile);
    }
}
