using ProgramInstaller.Controllers;
using ProgramInstaller.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ProgramInstaller;

public partial class MainWindow : Window
{
    private readonly ConfigController _configController = new();
    private Programas _programas = new();
    private bool _isInstalling;
    private bool _hashOverrideCleanupFailed;

    public MainWindow()
    {
        InitializeComponent();

        opt64bits.IsChecked = Environment.Is64BitOperatingSystem;
        opt32bits.IsChecked = !Environment.Is64BitOperatingSystem;

        LoadPrograms();
    }

    private async void btnInstalar_Click(object sender, RoutedEventArgs e)
    {
        List<Programa> ativos = _programas.ListaProgramas
            .Where(programa => programa.Ativo && IsArchitectureCompatible(programa))
            .ToList();

        if (ativos.Count == 0)
        {
            MessageBox.Show(
                "Não há programas ativos compatíveis com a arquitetura selecionada.",
                "Nada para executar",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        await ExecuteProgramsAsync(ativos, "Execução dos programas ativos");
    }

    private async void btnExecutarLinha_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: Programa programa } || _isInstalling)
            return;

        if (!IsArchitectureCompatible(programa))
        {
            MessageBox.Show(
                $"{programa.Nome} não está configurado para a arquitetura selecionada.",
                "Arquitetura incompatível",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        if (!programa.Ativo)
        {
            MessageBoxResult confirmation = MessageBox.Show(
                $"{programa.Nome} está inativo e não participa da execução em lote. Deseja executá-lo mesmo assim?",
                "Executar programa inativo",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmation != MessageBoxResult.Yes)
                return;
        }

        await ExecuteProgramsAsync([programa], $"Execução individual: {programa.Nome}");
    }

    private void btnConfig_Click(object sender, RoutedEventArgs e)
    {
        Config configWindow = new() { Owner = this };
        configWindow.ShowDialog();
        LoadPrograms();
    }

    private void btnLimparLog_Click(object sender, RoutedEventArgs e) =>
        txtProgresso.Clear();

    private void LoadPrograms()
    {
        _programas = _configController.Load();
        dtProgramas.ItemsSource = _programas.ListaProgramas;
        UpdateSummary();
    }

    private async Task ExecuteProgramsAsync(IReadOnlyCollection<Programa> programas, string operationName)
    {
        if (_isInstalling)
            return;

        _isInstalling = true;
        _hashOverrideCleanupFailed = false;
        SetBusyState(true);
        txtProgresso.Clear();
        txtStatusGeral.Text = operationName;

        foreach (Programa programa in programas)
            programa.StatusExecucao = "Na fila";

        int successful = 0;
        int failed = 0;

        List<Programa> executionQueue = programas.ToList();

        try
        {
            for (int index = 0; index < executionQueue.Count; index++)
            {
                Programa programa = executionQueue[index];

                if (await ExecuteProgramAsync(programa))
                    successful++;
                else
                    failed++;

                if (!_hashOverrideCleanupFailed)
                    continue;

                foreach (Programa pending in executionQueue.Skip(index + 1))
                    pending.StatusExecucao = "Cancelado por segurança";

                failed += executionQueue.Count - index - 1;
                break;
            }

            txtStatusGeral.Text = _hashOverrideCleanupFailed
                ? "Interrompido: não foi possível restaurar a proteção de hash"
                : failed == 0
                    ? $"Concluído: {successful} programa(s) executado(s)"
                    : $"Concluído com alertas: {successful} sucesso(s), {failed} falha(s)";
        }
        finally
        {
            _isInstalling = false;
            SetBusyState(false);
        }
    }

    private async Task<bool> ExecuteProgramAsync(Programa programa)
    {
        AppendLog($"Iniciando {programa.Nome}...");
        bool requiresHashOverride =
            programa.PermitirHashDiferente && IsWingetCommand(programa.Caminho);
        bool hashOverrideEnabled = false;
        bool executionSuccessful = false;

        try
        {
            if (requiresHashOverride)
            {
                programa.StatusExecucao = "Liberando hash";
                AppendLog($"{programa.Nome}: habilitando temporariamente a exceção de hash do WinGet.");

                if (!await SetInstallerHashOverrideAsync(enabled: true))
                {
                    programa.StatusExecucao = "Falhou ao liberar hash";
                    AppendLog($"{programa.Nome} não foi executado porque a exceção de hash não pôde ser habilitada.");
                    return false;
                }

                hashOverrideEnabled = true;
            }

            programa.StatusExecucao = "Executando";
            string arguments = WingetSecurityOptions.RemoveIgnoreSecurityHash(programa.Argumentos);

            if (requiresHashOverride)
                arguments = WingetSecurityOptions.AddIgnoreSecurityHash(arguments);

            ProcessResult result = await RunProcessAsync(programa.Caminho, arguments);

            if (!string.IsNullOrWhiteSpace(result.StandardOutput))
                AppendLog(result.StandardOutput.Trim());

            if (result.ExitCode == 0)
            {
                executionSuccessful = true;
                programa.StatusExecucao = hashOverrideEnabled ? "Restaurando segurança" : "Concluído";
                AppendLog($"{programa.Nome} concluído com sucesso.");
            }
            else
            {
                programa.StatusExecucao = $"Falhou ({result.ExitCode})";
                AppendLog($"Falha em {programa.Nome}: {GetProcessError(result)}");
            }
        }
        catch (Win32Exception) when (IsWingetCommand(programa.Caminho))
        {
            programa.StatusExecucao = "WinGet indisponível";
            AppendLog("WinGet não foi encontrado. Instale ou registre o App Installer do Windows e tente novamente.");
        }
        catch (Exception ex)
        {
            programa.StatusExecucao = "Falhou";
            AppendLog($"Falha em {programa.Nome}: {ex.Message}");
        }
        finally
        {
            if (hashOverrideEnabled)
            {
                bool protectionRestored = await SetInstallerHashOverrideAsync(enabled: false);

                if (!protectionRestored)
                    protectionRestored = await SetInstallerHashOverrideAsync(enabled: false);

                if (protectionRestored)
                {
                    AppendLog("Proteção de hash do WinGet restaurada.");

                    if (executionSuccessful)
                        programa.StatusExecucao = "Concluído";
                }
                else
                {
                    executionSuccessful = false;
                    _hashOverrideCleanupFailed = true;
                    programa.StatusExecucao = "Proteção de hash pendente";
                    AppendLog("ATENÇÃO: não foi possível desabilitar InstallerHashOverride. O lote foi interrompido.");
                    MessageBox.Show(
                        "Não foi possível restaurar a proteção de hash do WinGet.\n\n" +
                        "Execute como administrador:\nwinget settings --disable InstallerHashOverride",
                        "Atenção de segurança",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        return executionSuccessful;
    }

    private async Task<bool> SetInstallerHashOverrideAsync(bool enabled)
    {
        string action = enabled ? "enable" : "disable";

        try
        {
            ProcessResult result = await RunProcessAsync(
                "winget",
                $"settings --{action} InstallerHashOverride");

            if (result.ExitCode == 0)
                return true;

            AppendLog($"WinGet não conseguiu {(enabled ? "habilitar" : "desabilitar")} a exceção de hash: {GetProcessError(result)}");
            return false;
        }
        catch (Exception ex)
        {
            AppendLog($"Falha ao {(enabled ? "habilitar" : "desabilitar")} a exceção de hash: {ex.Message}");
            return false;
        }
    }

    private static string GetProcessError(ProcessResult result) =>
        !string.IsNullOrWhiteSpace(result.StandardError)
            ? result.StandardError.Trim()
            : !string.IsNullOrWhiteSpace(result.StandardOutput)
                ? result.StandardOutput.Trim()
                : "O processo não informou detalhes.";

    private static async Task<ProcessResult> RunProcessAsync(string fileName, string arguments)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new InvalidOperationException("O comando ou caminho do programa não foi informado.");

        using Process process = new()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName.Trim(),
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        if (!process.Start())
            throw new InvalidOperationException("Não foi possível iniciar o processo.");

        Task<string> standardOutput = process.StandardOutput.ReadToEndAsync();
        Task<string> standardError = process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();
        await Task.WhenAll(standardOutput, standardError);

        return new ProcessResult(process.ExitCode, standardOutput.Result, standardError.Result);
    }

    private bool IsArchitectureCompatible(Programa programa) =>
        (opt32bits.IsChecked == true && programa.x86 == "S") ||
        (opt64bits.IsChecked == true && programa.x64 == "S");

    private static bool IsWingetCommand(string command) =>
        command.Trim().Equals("winget", StringComparison.OrdinalIgnoreCase) ||
        command.Trim().Equals("winget.exe", StringComparison.OrdinalIgnoreCase);

    private void SetBusyState(bool isBusy)
    {
        dtProgramas.IsEnabled = !isBusy;
        btnInstalar.IsEnabled = !isBusy;
        btnConfig.IsEnabled = !isBusy;
        opt32bits.IsEnabled = !isBusy;
        opt64bits.IsEnabled = !isBusy;
    }

    private void UpdateSummary()
    {
        int active = _programas.ListaProgramas.Count(programa => programa.Ativo);
        int inactive = _programas.ListaProgramas.Count - active;

        txtAtivos.Text = $"{active} ativo(s)";
        txtInativos.Text = $"{inactive} inativo(s)";
        emptyMain.Visibility = _programas.ListaProgramas.Count == 0
            ? Visibility.Visible
            : Visibility.Collapsed;
        btnInstalar.IsEnabled = active > 0;
    }

    private void AppendLog(string message)
    {
        txtProgresso.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
        txtProgresso.ScrollToEnd();
    }

    private sealed record ProcessResult(int ExitCode, string StandardOutput, string StandardError);
}
