using System.Text.RegularExpressions;

namespace ProgramInstaller.Controllers;

internal static class WingetSecurityOptions
{
    private const string IgnoreSecurityHashFlag = "--ignore-security-hash";
    private static readonly Regex IgnoreSecurityHashPattern = new(
        @"(?<!\S)--ignore-security-hash(?!\S)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex RepeatedWhitespace = new(
        @"\s{2,}",
        RegexOptions.Compiled);

    public static bool ContainsIgnoreSecurityHash(string arguments) =>
        IgnoreSecurityHashPattern.IsMatch(arguments ?? string.Empty);

    public static string RemoveIgnoreSecurityHash(string arguments) =>
        RepeatedWhitespace.Replace(
            IgnoreSecurityHashPattern.Replace(arguments ?? string.Empty, string.Empty),
            " ").Trim();

    public static string AddIgnoreSecurityHash(string arguments)
    {
        string normalized = RemoveIgnoreSecurityHash(arguments);
        return string.IsNullOrWhiteSpace(normalized)
            ? IgnoreSecurityHashFlag
            : $"{normalized} {IgnoreSecurityHashFlag}";
    }
}
