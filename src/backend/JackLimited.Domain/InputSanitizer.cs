using System.Text.RegularExpressions;

namespace JackLimited.Domain;

/// <summary>
/// Utility class for input sanitization to prevent XSS and other injection attacks.
/// </summary>
public static class InputSanitizer
{
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
    /// Sanitizes email input by trimming and converting to lowercase.
    /// </summary>
    /// <param name="email">The email to sanitize.</param>
    /// <returns>The sanitized email.</returns>
    public static string? SanitizeEmail(string? email)
    {
        return string.IsNullOrEmpty(email) ? null : email.Trim().ToLowerInvariant();
    }
}