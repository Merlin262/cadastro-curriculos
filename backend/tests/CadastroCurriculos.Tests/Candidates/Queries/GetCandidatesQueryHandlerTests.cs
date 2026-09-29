using CadastroCurriculos.Application.Candidates.Queries.GetCandidateById;
using CadastroCurriculos.Application.Candidates.Queries.GetCandidates;
using CadastroCurriculos.Domain.Candidates;
using CadastroCurriculos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CadastroCurriculos.Tests.Candidates.Queries;

public class GetCandidatesQueryHandlerTests
{
    private static ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Handle_ReturnsCandidates_MostRecentFirst()
    {
        await using var dbContext = CreateInMemoryDbContext();

        var older = new Candidate("Ana Costa", "ana@example.com", null, null, null, CandidateSource.Manual, null);
        await Task.Delay(10);
        var newer = new Candidate("Bruno Lima", "bruno@example.com", null, null, null, CandidateSource.Pdf, "curriculo.pdf");

        dbContext.Candidates.AddRange(older, newer);
        await dbContext.SaveChangesAsync();

        var handler = new GetCandidatesQueryHandler(dbContext);
        var result = await handler.Handle(new GetCandidatesQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(newer.Id, result[0].Id);
        Assert.Equal(older.Id, result[1].Id);
    }

    [Fact]
    public async Task Handle_ReturnsEmptyList_WhenNoCandidatesExist()
    {
        await using var dbContext = CreateInMemoryDbContext();
        var handler = new GetCandidatesQueryHandler(dbContext);

        var result = await handler.Handle(new GetCandidatesQuery(), CancellationToken.None);

        Assert.Empty(result);
    }
}

public class GetCandidateByIdQueryHandlerTests
{
    private static ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Handle_ReturnsNull_WhenCandidateDoesNotExist()
    {
        await using var dbContext = CreateInMemoryDbContext();
        var handler = new GetCandidateByIdQueryHandler(dbContext);

        var result = await handler.Handle(new GetCandidateByIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_ReturnsFullDetails_WhenCandidateExists()
    {
        await using var dbContext = CreateInMemoryDbContext();
        var candidate = new Candidate(
            "Ana Costa", "ana@example.com", "(41) 90000-0000", "Dados", "Resumo profissional", CandidateSource.Manual, null);
        dbContext.Candidates.Add(candidate);
        await dbContext.SaveChangesAsync();

        var handler = new GetCandidateByIdQueryHandler(dbContext);
        var result = await handler.Handle(new GetCandidateByIdQuery(candidate.Id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Ana Costa", result!.FullName);
        Assert.Equal("Resumo profissional", result.ProfessionalSummary);
    }
}
