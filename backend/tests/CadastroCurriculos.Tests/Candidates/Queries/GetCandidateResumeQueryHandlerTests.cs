using System.Text;
using CadastroCurriculos.Application.Candidates.Queries.GetCandidateResume;
using CadastroCurriculos.Domain.Candidates;
using CadastroCurriculos.Infrastructure.Persistence.Repositories;
using CadastroCurriculos.Tests.TestSupport;

namespace CadastroCurriculos.Tests.Candidates.Queries;

public class GetCandidateResumeQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsNull_WhenCandidateDoesNotExist()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var repository = new CandidateRepository(dbContext);
        var handler = new GetCandidateResumeQueryHandler(repository);

        var result = await handler.Handle(new GetCandidateResumeQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_ReturnsNull_WhenCandidateHasNoResumeFile()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var repository = new CandidateRepository(dbContext);
        var candidate = new Candidate("Ana Costa", "ana@example.com", null, null, null, CandidateSource.Manual, null);
        dbContext.Candidates.Add(candidate);
        await dbContext.SaveChangesAsync();

        var handler = new GetCandidateResumeQueryHandler(repository);
        var result = await handler.Handle(new GetCandidateResumeQuery(candidate.Id), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_ReturnsFileContentAndName_WhenResumeFileExists()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var repository = new CandidateRepository(dbContext);
        var fileContent = Encoding.ASCII.GetBytes("%PDF-1.7\nconteudo");
        var candidate = new Candidate(
            "Bruno Lima", "bruno@example.com", null, null, null, CandidateSource.Pdf, "curriculo.pdf", fileContent);
        dbContext.Candidates.Add(candidate);
        await dbContext.SaveChangesAsync();

        var handler = new GetCandidateResumeQueryHandler(repository);
        var result = await handler.Handle(new GetCandidateResumeQuery(candidate.Id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(fileContent, result!.Content);
        Assert.Equal("curriculo.pdf", result.FileName);
    }
}
