using CadastroCurriculos.Application.Candidates.Commands.CreateCandidate;
using CadastroCurriculos.Domain.Candidates;
using CadastroCurriculos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CadastroCurriculos.Tests.Candidates.Commands;

public class CreateCandidateCommandHandlerTests
{
    private static ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Handle_PersistsCandidate_AndReturnsMatchingDto()
    {
        await using var dbContext = CreateInMemoryDbContext();
        var handler = new CreateCandidateCommandHandler(dbContext);

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
        await using var dbContext = CreateInMemoryDbContext();
        var handler = new CreateCandidateCommandHandler(dbContext);

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
