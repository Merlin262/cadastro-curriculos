using System.Text.RegularExpressions;

namespace CadastroCurriculos.Application.Candidates.Parsing;

public sealed record ParsedResumeFields(string? FullName, string? Email, string? Phone);

/// <summary>
/// Heuristic, regex-based extraction of name/e-mail/phone from raw resume text.
/// Extraction is best-effort: it will not work for every resume layout, which is why
/// the caller always lets the user review/correct the result before saving (see DESENVOLVIMENTO.md).
/// </summary>
public static class ResumeTextParser
{
    private static readonly Regex EmailRegex = new(
        @"[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)+",
        RegexOptions.Compiled);

    // Brazilian phone numbers, with or without country code / DDD parentheses / dashes.
    private static readonly Regex PhoneRegex = new(
        @"(?:\+?55\s?)?\(?\d{2}\)?[\s.-]?\d{4,5}[\s.-]?\d{4}",
        RegexOptions.Compiled);

    private static readonly string[] SectionHeaderWords =
    {
        "curriculo", "currículo", "resumo", "objetivo", "experiencia", "experiência",
        "formacao", "formação", "educacao", "educação", "habilidades", "competencias",
        "competências", "contato", "contact", "summary", "resume", "profile", "perfil",
        "sobre", "about", "endereco", "endereço", "idiomas", "languages",
    };

    public static ParsedResumeFields Parse(string text)
    {
        var email = ExtractEmail(text);
        var phone = ExtractPhone(text);
        var fullName = ExtractFullName(text);

        return new ParsedResumeFields(fullName, email, phone);
    }

    public static string? ExtractEmail(string text)
    {
        var match = EmailRegex.Match(text);
        return match.Success ? match.Value.Trim().TrimEnd('.', ',') : null;
    }

    public static string? ExtractPhone(string text)
    {
        foreach (Match match in PhoneRegex.Matches(text))
        {
            var digits = Regex.Replace(match.Value, @"\D", string.Empty);
            // Discard matches too short/long to plausibly be a phone number
            // (avoids grabbing CPF/CEP-like digit runs that happen to fit the pattern).
            if (digits.Length is >= 10 and <= 13)
            {
                return match.Value.Trim();
            }
        }

        return null;
    }

    /// <summary>
    /// Assumes the candidate's name is near the top of the document and looks like a
    /// short, capitalized line without digits, e-mails or common section-header words.
    /// This works for typical single-column resumes but can miss creative layouts,
    /// tables, or names embedded in a header image (not extractable as text at all).
    /// </summary>
    public static string? ExtractFullName(string text)
    {
        var lines = text
            .Split('\n')
            .Select(l => l.Trim())
            .Where(l => l.Length > 0)
            .Take(20)
            .ToList();

        foreach (var line in lines)
        {
            if (IsLikelyNameLine(line))
            {
                return NormalizeName(line);
            }
        }

        return null;
    }

    private static bool IsLikelyNameLine(string line)
    {
        if (line.Length < 4 || line.Length > 60)
        {
            return false;
        }

        if (line.Any(char.IsDigit) || line.Contains('@'))
        {
            return false;
        }

        var lowerLine = line.ToLowerInvariant();
        if (SectionHeaderWords.Any(word => lowerLine.Contains(word)))
        {
            return false;
        }

        var words = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length is < 2 or > 6)
        {
            return false;
        }

        // Every "word" should be alphabetic (allowing accents and apostrophes),
        // which filters out addresses, titles with punctuation, etc.
        return words.All(w => w.All(c => char.IsLetter(c) || c is '\'' or '.'));
    }

    private static string NormalizeName(string line)
    {
        var words = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var titleCased = words.Select(w =>
            w.Length == 0 ? w : char.ToUpperInvariant(w[0]) + w[1..].ToLowerInvariant());
        return string.Join(' ', titleCased);
    }
}
