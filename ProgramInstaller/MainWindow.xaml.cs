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
    public MainWindow()
    {
        InitializeComponent();
        GetDados();
    }

    private async void Button_Click(object sender, RoutedEventArgs e)
    {
        await IniciarInstalcao();
    }

    private async Task IniciarInstalcao()
    {
        try {
            if (chk32bits.IsChecked == true || chk64bits.IsChecked == true) {
                dtProgramas.Visibility = Visibility.Hidden;
                btnConfig.Visibility = Visibility.Hidden;
                btnInstalar.Visibility = Visibility.Hidden;
                txtProgresso.Visibility = Visibility.Visible;

                foreach (System.Data.DataRowView dt in dtProgramas.ItemsSource) {
                    if (dt.Row.ItemArray[4].ToString() == "S" && chk32bits.IsChecked == true) {
                        txtProgresso.AppendText("Instalando " + dt.Row.ItemArray[1].ToString() + ". \n");
                        await Task.Run(() => Process.Start(dt.Row.ItemArray[2].ToString(), dt.Row.ItemArray[3].ToString()).WaitForExit());
                    }
                    else if (dt.Row.ItemArray[5].ToString() == "S" && chk64bits.IsChecked == true) {
                        txtProgresso.AppendText("Instalando " + dt.Row.ItemArray[1].ToString() + ". \n");
                        await Task.Run(() => Process.Start(dt.Row.ItemArray[2].ToString(), dt.Row.ItemArray[3].ToString()).WaitForExit());
                    }
                }

                txtProgresso.AppendText("Concluído!");
                btnOK.Visibility = Visibility.Visible;
            }
            else {
                MessageBox.Show("Selecione a arquitetura do S.O.");
            }
        }
        catch (Exception ex) {

            MessageBox.Show(ex.Message);
        }

    }

    private void chk32bits_Checked(object sender, RoutedEventArgs e) => chk64bits.IsChecked = false;

    private void chk64bits_Checked(object sender, RoutedEventArgs e) => chk32bits.IsChecked = false;


    private void Button_Click_1(object sender, RoutedEventArgs e)
    {
        Config Config = new Config();
        this.Hide();
        Config.Show();
    }

    private void GetDados()
    {
        try {
            DataSet dsResultado = new DataSet();
            dsResultado.ReadXml(@"config\config.xml");
            if (dsResultado.Tables.Count != 0) {
                if (dsResultado.Tables[0].Rows.Count > 0) {
                    dtProgramas.ItemsSource = new DataView(dsResultado.Tables["Programa"]);
                }
            }
        }
        catch (Exception ex) {
            throw ex;
        }
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        Environment.Exit(0);
    }

    private void Button_Click_2(object sender, RoutedEventArgs e)
    {
        btnOK.Visibility = Visibility.Hidden;
        txtProgresso.Visibility = Visibility.Hidden;
        dtProgramas.Visibility = Visibility.Visible;
        btnConfig.Visibility = Visibility.Visible;
        btnInstalar.Visibility = Visibility.Visible;

        txtProgresso.Text = string.Empty;
    }
}
