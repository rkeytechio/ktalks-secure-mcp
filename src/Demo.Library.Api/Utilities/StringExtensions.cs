namespace Demo.Library.Api.Utilities;

internal static class StringExtensions
{
    public static string EnsureStartsWithSlash(this string? value, string defaultValue)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        return value.StartsWith('/') ? value : "/" + value;
    }
}
