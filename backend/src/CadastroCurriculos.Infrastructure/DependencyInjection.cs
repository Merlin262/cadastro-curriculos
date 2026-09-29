using CadastroCurriculos.Domain.Candidates;
using CadastroCurriculos.Infrastructure.Persistence;
using CadastroCurriculos.Infrastructure.Persistence.Repositories;
using CadastroCurriculos.Infrastructure.Resumes;
using CadastroCurriculos.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CadastroCurriculos.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("A connection string 'DefaultConnection' não foi configurada.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<ICandidateRepository, CandidateRepository>();
        services.AddScoped<IResumeTextExtractor, PdfPigResumeTextExtractor>();

        return services;
    }
}
