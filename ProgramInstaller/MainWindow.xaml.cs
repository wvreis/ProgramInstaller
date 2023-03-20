using ProgramInstaller.Controllers;
using ProgramInstaller.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

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
        dtProgramas.ItemsSource = Programas?.ListaProgramas;
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

    async Task StartProgram()
    {
        if (!isArquituraChecked()) {
            MessageBox.Show("Selecione a arquitetura do S.O.");
            return;
        }

        isInstalling = true;

        ChangeFieldsVisibility();

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
                    txtProgresso.AppendText($"Instalando {programa.Nome ?? string.Empty}. \n");

                    await Task.Run(() =>
                        Process.Start(
                            programa.Caminho ?? string.Empty,
                            programa.Argumentos ?? string.Empty)
                        .WaitForExit());
                }
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
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

    #region VALIDATIONS
    bool isArquituraChecked() =>
        chk32bits.IsChecked == true || chk64bits.IsChecked == true;

    bool is32bitsCommand(Programa programa) =>
        programa.x86 == "S" && chk32bits.IsChecked == true;

    bool is64BitsCommand(Programa programa) =>
        programa.x64 == "S" && chk64bits.IsChecked == true;
    #endregion
}
