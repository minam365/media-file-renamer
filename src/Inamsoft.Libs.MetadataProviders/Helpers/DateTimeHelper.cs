using System.Globalization;

namespace Inamsoft.Libs.MetadataProviders.Helpers;

internal static class DateTimeHelper
{
    private static readonly string[] _datePatterns =
        {
            "yyyy:MM:dd HH:mm:ss.fff",
            "yyyy:MM:dd HH:mm:ss",
            "yyyy:MM:dd HH:mm",
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-dd HH:mm",
            "yyyy.MM.dd HH:mm:ss",
            "yyyy.MM.dd HH:mm",
            "yyyy-MM-ddTHH:mm:ss.fff",
            "yyyy-MM-ddTHH:mm:ss.ff",
            "yyyy-MM-ddTHH:mm:ss.f",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-ddTHH:mm.fff",
            "yyyy-MM-ddTHH:mm.ff",
            "yyyy-MM-ddTHH:mm.f",
            "yyyy-MM-ddTHH:mm",
            // Example: "Thu Nov 20 06:45:05 2025"
            "ddd MMM dd HH:mm:ss yyyy",
            "yyyy:MM:dd",
            "yyyy-MM-dd",
            "yyyy-MM",
            "yyyyMMdd", // as used in IPTC data
            "yyyy"
        };

    private static readonly IFormatProvider s_invariant = CultureInfo.InvariantCulture;

    public static DateTime? ParseDateTime(string dateTimeString)
    {
        if (string.IsNullOrWhiteSpace(dateTimeString))
            return null;

        // Trim once to avoid repeated allocations inside parsing attempts
        var input = dateTimeString.Trim();

        // Try exact parse against all known patterns in a single call (more efficient than looping)
        if (DateTime.TryParseExact(input, _datePatterns, s_invariant, DateTimeStyles.AllowWhiteSpaces, out var parsed))
        {
            return parsed;
        }

        // Fallback to general parsing using invariant culture to reduce culture-dependent surprises
        if (DateTime.TryParse(input, s_invariant, DateTimeStyles.None, out var fallback))
        {
            return fallback;
        }

        return null;
    }

    /// <summary>
    /// Attempts to parse the provided date/time string into a <see cref="DateTime"/> value.
    /// </summary>
    /// <param name="dateTimeString">The input string to parse.</param>
    /// <param name="result">When this method returns, contains the parsed <see cref="DateTime"/> value if parsing succeeded; otherwise <c>DateTime.MinValue</c>.</param>
    /// <returns><c>true</c> if parsing succeeded; otherwise <c>false</c>.</returns>
    public static bool TryParseDateTime(string dateTimeString, out DateTime result)
    {
        result = DateTime.MinValue;
        if (string.IsNullOrWhiteSpace(dateTimeString))
            return false;

        var input = dateTimeString.Trim();

        if (DateTime.TryParseExact(input, _datePatterns, s_invariant, DateTimeStyles.AllowWhiteSpaces, out var parsed))
        {
            result = parsed;
            return true;
        }

        if (DateTime.TryParse(input, s_invariant, DateTimeStyles.None, out var fallback))
        {
            result = fallback;
            return true;
        }

        return false;
    }
}
