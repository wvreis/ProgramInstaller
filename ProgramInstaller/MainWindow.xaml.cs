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
        SetBusyState(true);
        txtProgresso.Clear();
        txtStatusGeral.Text = operationName;

        foreach (Programa programa in programas)
            programa.StatusExecucao = "Na fila";

        int successful = 0;
        int failed = 0;

        try
        {
            foreach (Programa programa in programas)
            {
                if (await ExecuteProgramAsync(programa))
                    successful++;
                else
                    failed++;
            }

            txtStatusGeral.Text = failed == 0
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
        programa.StatusExecucao = "Executando";
        AppendLog($"Iniciando {programa.Nome}...");

        try
        {
            ProcessResult result = await RunProcessAsync(programa.Caminho, programa.Argumentos);

            if (!string.IsNullOrWhiteSpace(result.StandardOutput))
                AppendLog(result.StandardOutput.Trim());

            if (result.ExitCode == 0)
            {
                programa.StatusExecucao = "Concluído";
                AppendLog($"{programa.Nome} concluído com sucesso.");
                return true;
            }

            programa.StatusExecucao = $"Falhou ({result.ExitCode})";
            string error = string.IsNullOrWhiteSpace(result.StandardError)
                ? "O processo não informou detalhes."
                : result.StandardError.Trim();
            AppendLog($"Falha em {programa.Nome}: {error}");
            return false;
        }
        catch (Win32Exception) when (IsWingetCommand(programa.Caminho))
        {
            programa.StatusExecucao = "WinGet indisponível";
            AppendLog("WinGet não foi encontrado. Instale ou registre o App Installer do Windows e tente novamente.");
            return false;
        }
        catch (Exception ex)
        {
            programa.StatusExecucao = "Falhou";
            AppendLog($"Falha em {programa.Nome}: {ex.Message}");
            return false;
        }
    }

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
        command.Contains("winget", StringComparison.OrdinalIgnoreCase);

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
