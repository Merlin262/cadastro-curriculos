namespace CadastroCurriculos.Domain.Candidates;

public enum CandidateSource
{
    Manual = 0,
    Pdf = 1,
}

public sealed class Candidate
{
    public Guid Id { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public string? AreaOfInterest { get; private set; }
    public string? ProfessionalSummary { get; private set; }
    public CandidateSource Source { get; private set; }
    public string? ResumeFileName { get; private set; }
    public byte[]? ResumeFileContent { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public bool HasResumeFile => ResumeFileContent is not null;

    private Candidate()
    {
    }

    public Candidate(
        string fullName,
        string email,
        string? phone,
        string? areaOfInterest,
        string? professionalSummary,
        CandidateSource source,
        string? resumeFileName,
        byte[]? resumeFileContent = null)
    {
        Id = Guid.NewGuid();
        FullName = fullName.Trim();
        Email = email.Trim();
        Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        AreaOfInterest = string.IsNullOrWhiteSpace(areaOfInterest) ? null : areaOfInterest.Trim();
        ProfessionalSummary = string.IsNullOrWhiteSpace(professionalSummary) ? null : professionalSummary.Trim();
        Source = source;
        ResumeFileName = resumeFileName;
        ResumeFileContent = resumeFileContent;
        CreatedAtUtc = DateTime.UtcNow;
    }
}
