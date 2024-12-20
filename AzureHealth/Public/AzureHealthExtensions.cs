namespace Muthink.AzureHealth;

/// <summary>
/// Helpful AzureHealth extensions
/// </summary>
public static class AzureHealthExtensions
{
    /// <summary>
    ///     Truncates a string, efficiently.
    /// </summary>
    /// <param name="string">The string</param>
    /// <param name="maxLength">No longer than this length</param>
    /// <returns>
    ///     The original <paramref name="string" /> to a maximum
    ///     length of <paramref name="maxLength" />
    /// </returns>
    public static string Truncate(this string @string, int maxLength)
        => @string.Length <= maxLength ? @string : @string[..maxLength];
}