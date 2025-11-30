using System.Text.RegularExpressions;

namespace JackLimited.Domain;

public class Survey
{
    public Guid Id { get; set; }
    public int LikelihoodToRecommend { get; set; }
    public string? Comments { get; set; }
    public string? Email { get; set; }
    public DateTime CreatedAt { get; set; }

    // Domain validation
    public bool IsValid()
    {
        return LikelihoodToRecommend >= 0 && LikelihoodToRecommend <= 10 &&
               (Comments == null || Comments.Length <= 1000) &&
               (Email == null || IsValidEmail(Email));
    }

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;

        try
        {
            var emailRegex = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(email, emailRegex);
        }
        catch
        {
            return false;
        }
    }

    // Sanitization method
    public void Sanitize()
    {
        if (!string.IsNullOrEmpty(Comments))
        {
            Comments = SanitizeText(Comments);
        }

        if (!string.IsNullOrEmpty(Email))
        {
            Email = Email.Trim().ToLowerInvariant();
        }
    }

    private string SanitizeText(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        // Remove HTML tags and entities
        var sanitized = Regex.Replace(text, @"<[^>]*>", "");
        sanitized = Regex.Replace(sanitized, @"&[^;]+;", "");

        // Remove control characters
        sanitized = Regex.Replace(sanitized, @"[\x00-\x1F\x7F-\x9F]", "");

        return sanitized.Trim();
    }
}