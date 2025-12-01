using System.Linq;
using System.Text.RegularExpressions;

namespace JackLimited.Domain;

/// <summary>
/// Utility class for input sanitization to prevent XSS and other injection attacks.
/// </summary>
public static class InputSanitizer
{
    private static readonly Regex[] UnsafeTextPatterns =
    {
        new(@"<script[^>]*>.*?</script>", RegexOptions.IgnoreCase | RegexOptions.Singleline),
        new(@"<[^>]+>", RegexOptions.IgnoreCase | RegexOptions.Singleline),
        new(@"javascript:", RegexOptions.IgnoreCase),
        new(@"on\w+\s*=", RegexOptions.IgnoreCase),
    };

    /// <summary>
    /// Sanitizes text input by removing HTML tags, entities, and control characters.
    /// </summary>
    /// <param name="text">The text to sanitize.</param>
    /// <returns>The sanitized text.</returns>
    public static string SanitizeText(string? text)
    {
        if (string.IsNullOrEmpty(text)) return text ?? "";

        // Remove HTML tags
        var sanitized = Regex.Replace(text, @"<[^>]*>", "");

        // Remove HTML entities
        sanitized = Regex.Replace(sanitized, @"&[^;]+;", "");

        // Remove control characters
        sanitized = Regex.Replace(sanitized, @"[\x00-\x1F\x7F-\x9F]", "");

        // Trim whitespace
        return sanitized.Trim();
    }

    /// <summary>
    /// Determines whether the provided text contains potentially unsafe content.
    /// </summary>
    public static bool IsSafeText(string? text)
    {
        if (string.IsNullOrEmpty(text)) return true;

        return UnsafeTextPatterns.All(pattern => !pattern.IsMatch(text));
    }

    /// <summary>
    /// Sanitizes email input by trimming and converting to lowercase.
    /// </summary>
    /// <param name="email">The email to sanitize.</param>
    /// <returns>The sanitized email.</returns>
    public static string? SanitizeEmail(string? email)
    {
        return string.IsNullOrEmpty(email) ? null : email.Trim().ToLowerInvariant();
    }
}