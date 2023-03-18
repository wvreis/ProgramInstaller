using ProgramInstaller.Helpers;
using System;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace ProgramInstaller;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window {
    bool isInstalling = false;

    public MainWindow()
    {
        InitializeComponent();
        XmlHandler.GetDados(dtProgramas);
    }

    #region EVENTS
    private async void Button_Click_Instalar(object sender, RoutedEventArgs e)
    {
        await IniciarInstalcao();
    }

    private void Button_Click_Config(object sender, RoutedEventArgs e)
    {
        Config Config = new();
        Hide();
        Config.Show();
    }

    private void Button_Click_OK(object sender, RoutedEventArgs e) =>
       ChangeFieldsVisibility();

    private void chk32bits_Checked(object sender, RoutedEventArgs e) =>
        chk64bits.IsChecked = false;

    private void chk64bits_Checked(object sender, RoutedEventArgs e) =>
        chk32bits.IsChecked = false;

    private void Window_Closed(object sender, EventArgs e) =>
        Environment.Exit(0);

    #endregion

    async Task IniciarInstalcao()
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
        foreach (DataRowView dt in dtProgramas.ItemsSource) {
            try {
                if (is32bitsCommand(dt) || is64BitsCommand(dt)) {
                    txtProgresso.AppendText($"Instalando {dt.Row.ItemArray[1]?.ToString() ?? string.Empty}. \n");

                    await Task.Run(() =>
                        Process.Start(
                            dt.Row.ItemArray[2]?.ToString() ?? string.Empty,
                            dt.Row.ItemArray[3]?.ToString() ?? string.Empty)
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

    bool is32bitsCommand(DataRowView dt) =>
        dt.Row.ItemArray[4]?.ToString() == "S" && chk32bits.IsChecked == true;

    bool is64BitsCommand(DataRowView dt) =>
        dt.Row.ItemArray[5]?.ToString() == "S" && chk64bits.IsChecked == true;
    #endregion
}
