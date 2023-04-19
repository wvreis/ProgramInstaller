using ProgramInstaller.Controllers;
using ProgramInstaller.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace ProgramInstaller;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window {
    Programas? Programas { get; set; }
    bool isInstalling = false;

    public MainWindow()
    {
        InitializeComponent();
        Programas = new ConfigController().Load();
        dtProgramas.ItemsSource = Programas.ListaProgramas;
    }

    #region EVENTS
    private async void btnInstalar_Click(object sender, RoutedEventArgs e)
    {
        await StartProgram();
    }

    private void btnConfig_Click(object sender, RoutedEventArgs e)
    {
        Config Config = new();
        Hide();
        Config.Show();
    }

    private void btnOK_Click(object sender, RoutedEventArgs e) =>
       ChangeFieldsVisibility();

    private void chk32bits_Checked(object sender, RoutedEventArgs e) =>
        chk64bits.IsChecked = false;

    private void chk64bits_Checked(object sender, RoutedEventArgs e) =>
        chk32bits.IsChecked = false;

    private void Window_Closed(object sender, EventArgs e) =>
        Environment.Exit(0);

    #endregion

    #region AUXILIARY METHODS
    async Task StartProgram()
    {
        if (!IsArquituraChecked()) {
            MessageBox.Show("Selecione a arquitetura do S.O.");
            return;
        }

        isInstalling = true;

        ChangeFieldsVisibility();

        if (await IsWingetInstallNeeded())
            await WingetInstall();

        await ExecuteCommands();

        txtProgresso.AppendText("Concluído!");

        isInstalling = false;

        btnOK.Visibility = Visibility.Visible;
    }

    async Task ExecuteCommands()
    {
        foreach (Programa programa in dtProgramas.Items) {
            try {
                if (is32bitsCommand(programa) || is64BitsCommand(programa)) {
                    string output = string.Empty;

                    txtProgresso.AppendText($"Instalando {programa.Nome ?? string.Empty}... \n");

                    await Task.Run(() => {
                        Process process = new();

                        process.StartInfo.FileName = programa.Caminho ?? string.Empty;
                        process.StartInfo.Arguments = programa.Argumentos ?? string.Empty;

                        process.StartInfo.RedirectStandardError = true;

                        process.Start();
                        process.WaitForExit();

                        string errorOutput = process.StandardError.ReadToEnd();

                        int exitCode = process.ExitCode;

                        if (exitCode != 0)
                            output = $"{errorOutput} \n";
                    });

                    txtProgresso.AppendText(output);
                }
            }
            catch (Exception ex) {
                txtProgresso.AppendText($"{ex.Message} \n");
            }
        }
    }    

    void ChangeFieldsVisibility()
    {
        if (isInstalling) {
            dtProgramas.Visibility = Visibility.Hidden;
            btnConfig.Visibility = Visibility.Hidden;
            btnInstalar.Visibility = Visibility.Hidden;
            txtProgresso.Visibility = Visibility.Visible;
        }
        else {
            btnOK.Visibility = Visibility.Hidden;
            txtProgresso.Visibility = Visibility.Hidden;
            dtProgramas.Visibility = Visibility.Visible;
            btnConfig.Visibility = Visibility.Visible;
            btnInstalar.Visibility = Visibility.Visible;

            txtProgresso.Text = string.Empty;
        }
    }

    async Task<bool> IsWingetInstallNeeded()
    {
        try {
            bool hasWingetCommand = false;
            bool hasWingetInstalled = false;
            
            var listProgramas = dtProgramas.Items.SourceCollection as List<Programa>;

            hasWingetCommand = listProgramas.Where(x => x.Caminho.ToLower().Contains("winget")).Any();

            await Task.Run(() => {
                using (Process process = new()) {
                    process.StartInfo.FileName = "winget";
                    process.StartInfo.Arguments = "";
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;

                    process.Start();
                    process.WaitForExit();

                    string standardOutput = process.StandardOutput.ReadToEnd();
                    string errorOutput = process.StandardError.ReadToEnd();

                    if(standardOutput.ToLower().Contains("uso: winget"))
                        hasWingetInstalled = true;
                }
            });

            return hasWingetCommand && !hasWingetInstalled;
        }
        catch (Exception ex) {
            return true;
        }
    }

    async Task WingetInstall()
    {
        try {
            txtProgresso.AppendText($"Instalando Winget...\n");

            string output = string.Empty;

            output = await ExecuteCommand("powershell", "$ProgressPreference='Silent'");
            txtProgresso.AppendText(output);
            output = string.Empty;            

            output = await ExecuteCommand("powershell", "Invoke-WebRequest -Uri \"https://github.com/microsoft/winget-cli/releases/download/v1.1.12653/Microsoft.DesktopAppInstaller_8wekyb3d8bbwe.msixbundle\" -OutFile .\\WinGet.msixbundle\"");
            txtProgresso.AppendText(output);
            output = string.Empty;            

            output = await ExecuteCommand("powershell", "Invoke-WebRequest -Uri https://aka.ms/Microsoft.VCLibs.x64.14.00.Desktop.appx -OutFile Microsoft.VCLibs.x64.14.00.Desktop.appx");
            txtProgresso.AppendText(output);
            output = string.Empty;           

            output = await ExecuteCommand("powershell", "Add-AppxPackage Microsoft.VCLibs.x64.14.00.Desktop.appx");
            txtProgresso.AppendText(output);
            output = string.Empty;

            output = await ExecuteCommand("powershell", "Add-AppxPackage .\\WinGet.msixbundle");
            txtProgresso.AppendText(output);
            output = string.Empty;
        }
        catch (Exception ex) {
            txtProgresso.AppendText($"{ex.Message}\n");
        }
    }

    async Task<string> ExecuteCommand(string fileName, string arguments)
    {
        try {
            string output = string.Empty;

            await Task.Run(() => {
                using (Process process = new()) {
                    process.StartInfo.FileName = fileName;
                    process.StartInfo.Arguments = arguments;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;

                    process.Start();
                    process.WaitForExit();

                    string standardOutput = process.StandardOutput.ReadToEnd();
                    string errorOutput = process.StandardError.ReadToEnd();

                    int exitCode = process.ExitCode;

                    if (exitCode != 0)
                        output = $"{standardOutput} \n{errorOutput} \n";
                }
            });

            return output;
        }
        catch (Exception ex) {
            return $"{ex.Message}\n";
        }

    }
    #endregion

    #region VALIDATIONS
    bool IsArquituraChecked() =>
        chk32bits.IsChecked == true || chk64bits.IsChecked == true;

    bool is32bitsCommand(Programa programa) =>
        programa.x86 == "S" && chk32bits.IsChecked == true;

    bool is64BitsCommand(Programa programa) =>
        programa.x64 == "S" && chk64bits.IsChecked == true;
    #endregion
}
