using ProgramInstaller.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ProgramInstaller.Controllers;

public sealed class WingetCatalogService
{
    private static readonly Regex AnsiEscapeSequence = new(
        "\\u001B(?:[@-Z\\\\-_]|\\[[0-?]*[ -/]*[@-~])",
        RegexOptions.Compiled);

    public async Task<IReadOnlyList<WingetPackage>> SearchAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = "winget",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
            CreateNoWindow = true
        };

        string[] arguments =
        [
            "search",
            "--query", query,
            "--source", "winget",
            "--count", "50",
            "--accept-source-agreements",
            "--disable-interactivity"
        ];

        foreach (string argument in arguments)
            startInfo.ArgumentList.Add(argument);

        using Process process = new() { StartInfo = startInfo };

        try
        {
            if (!process.Start())
                throw new WingetCatalogUnavailableException("Não foi possível iniciar o WinGet.");
        }
        catch (Win32Exception ex)
        {
            throw new WingetCatalogUnavailableException(
                "O WinGet não foi encontrado. Instale ou atualize o App Installer do Windows.",
                ex);
        }

        Task<string> standardOutput = process.StandardOutput.ReadToEndAsync(cancellationToken);
        Task<string> standardError = process.StandardError.ReadToEndAsync(cancellationToken);

        try
        {
            await process.WaitForExitAsync(cancellationToken);
            await Task.WhenAll(standardOutput, standardError);
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);

            throw;
        }

        string output = await standardOutput;
        string error = await standardError;

        if (process.ExitCode != 0)
        {
            string details = string.IsNullOrWhiteSpace(error) ? output : error;
            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(details)
                    ? "O WinGet não conseguiu consultar o catálogo."
                    : details.Trim());
        }

        return ParseSearchOutput(output);
    }

    internal static IReadOnlyList<WingetPackage> ParseSearchOutput(string output)
    {
        string normalized = AnsiEscapeSequence.Replace(output, string.Empty).Replace("\r", string.Empty);
        string[] lines = normalized.Split('\n');
        int separatorIndex = Array.FindIndex(lines, IsTableSeparator);

        if (separatorIndex < 0)
            return [];

        List<WingetPackage> packages = [];

        foreach (string line in lines.Skip(separatorIndex + 1))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] columns = Regex.Split(line.Trim(), @"\s{2,}");

            if (columns.Length < 3 || string.IsNullOrWhiteSpace(columns[1]))
                continue;

            packages.Add(new WingetPackage(
                columns[0].Trim(),
                columns[1].Trim(),
                columns[2].Trim()));
        }

        return packages
            .DistinctBy(package => package.Id, StringComparer.OrdinalIgnoreCase)
            .Take(50)
            .ToList();
    }

    private static bool IsTableSeparator(string line)
    {
        string value = line.Trim();
        return value.Length >= 10 && value.All(character => character == '-');
    }
}

public sealed class WingetCatalogUnavailableException : Exception
{
    public WingetCatalogUnavailableException(string message)
        : base(message)
    {
    }

    public WingetCatalogUnavailableException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
