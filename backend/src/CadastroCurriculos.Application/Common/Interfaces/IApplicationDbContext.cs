using CadastroCurriculos.Domain.Candidates;
using Microsoft.EntityFrameworkCore;

namespace CadastroCurriculos.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Candidate> Candidates { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
