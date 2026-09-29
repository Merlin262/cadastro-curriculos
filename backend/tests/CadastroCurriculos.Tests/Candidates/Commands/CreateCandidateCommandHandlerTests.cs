using CadastroCurriculos.Application.Candidates.Commands.CreateCandidate;
using CadastroCurriculos.Domain.Candidates;
using CadastroCurriculos.Infrastructure.Persistence.Repositories;
using CadastroCurriculos.Tests.TestSupport;

namespace CadastroCurriculos.Tests.Candidates.Commands;

public class CreateCandidateCommandHandlerTests
{
    [Fact]
    public async Task Handle_PersistsCandidate_AndReturnsMatchingDto()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var repository = new CandidateRepository(dbContext);
        var handler = new CreateCandidateCommandHandler(repository);

        var command = new CreateCandidateCommand(
            "Maria Oliveira",
            "maria@example.com",
            "(11) 91234-5678",
            "Desenvolvimento Frontend",
            "5 anos de experiencia.",
            CandidateSource.Manual,
            null);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal("Maria Oliveira", result.FullName);
        Assert.Equal("maria@example.com", result.Email);
        Assert.Single(dbContext.Candidates);
        Assert.Equal(result.Id, dbContext.Candidates.Single().Id);
    }

    [Fact]
    public async Task Handle_TrimsWhitespace_FromFields()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var repository = new CandidateRepository(dbContext);
        var handler = new CreateCandidateCommandHandler(repository);

        var command = new CreateCandidateCommand(
            "  Maria Oliveira  ",
            "  maria@example.com  ",
            null,
            null,
            null,
            CandidateSource.Manual,
            null);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal("Maria Oliveira", result.FullName);
        Assert.Equal("maria@example.com", result.Email);
    }
}
