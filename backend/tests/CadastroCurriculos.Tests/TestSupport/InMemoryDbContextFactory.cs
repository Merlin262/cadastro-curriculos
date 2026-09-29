using CadastroCurriculos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CadastroCurriculos.Tests.TestSupport;

public static class InMemoryDbContextFactory
{
    public static ApplicationDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
