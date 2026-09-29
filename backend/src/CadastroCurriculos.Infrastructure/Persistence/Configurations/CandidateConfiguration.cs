using CadastroCurriculos.Domain.Candidates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CadastroCurriculos.Infrastructure.Persistence.Configurations;

public sealed class CandidateConfiguration : IEntityTypeConfiguration<Candidate>
{
    public void Configure(EntityTypeBuilder<Candidate> builder)
    {
        builder.ToTable("Candidates");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Phone)
            .HasMaxLength(30);

        builder.Property(c => c.AreaOfInterest)
            .HasMaxLength(150);

        builder.Property(c => c.ProfessionalSummary)
            .HasMaxLength(4000);

        builder.Property(c => c.ResumeFileName)
            .HasMaxLength(260);

        builder.Property(c => c.Source)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(c => c.Email);
        builder.HasIndex(c => c.CreatedAtUtc);
    }
}
