using ProgramInstaller.Controllers;
using ProgramInstaller.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace ProgramInstaller;
/// <summary>
/// Interaction logic for Config.xaml
/// </summary>
public partial class Config : Window {
    List<Programa>? Programas { get; set; } = new();
    Programa? SelectedPrograma { get; set; }
    int SelectedProgramaIndex { get; set; }

    public Config()
    {
        InitializeComponent();

        Programas = new ConfigController().Load();
        dtProgramas.ItemsSource = Programas;
        dtProgramas.SelectedIndex = 0;
    }

    #region EVENTS
    private void dtProgramas_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SelectedPrograma = (Programa?)dtProgramas.SelectedItem;
        SelectedProgramaIndex = dtProgramas.SelectedIndex;

        LoadFileds();
    }

    private void btnOk_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(txtId.Text)) {
            MessageBox.Show("Preencha o Campo Id!!!");
            return;
        }

        SaveFileds();

        if (dtProgramas.SelectedIndex != SelectedProgramaIndex)
            CleanFields();

        if (Title.Contains("Configuração - Inserindo Novo"))
            Programas?.Add(SelectedPrograma);

        DisableEdition();

        dtProgramas.Items.Refresh();

        new ConfigController().Save(Programas);

        dtProgramas.SelectedIndex = SelectedProgramaIndex;

        MessageBox.Show("Dados salvos com sucesso.");

        Title = "Configuração";

    }

    private void btnCancelar_Click(object sender, RoutedEventArgs e)
    {
        CleanFields();
        DisableEdition();
        LoadFileds();
        Title = "Configuração";
        dtProgramas.Items.Refresh();
    }

    private void btnNovo_Click(object sender, RoutedEventArgs e)
    {
        SelectedPrograma = new();

        CleanFields();
        EnableEdition();
        Title += " - Inserindo Novo";
    }

    private void btnAlterar_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(txtId.Text)) {
            MessageBox.Show("Selecione um registro clicando duas vezes nele!");
            return;
        }

        EnableEdition();
        Title += " - Alterando";
    }

    private void btnExcluir_Click(object sender, RoutedEventArgs e)
    {
        var dialogResult = MessageBox.Show("Deseja Confirmar a Exclusão do Registro Selecionado?", "Exclusão", MessageBoxButton.YesNo);

        if (dialogResult == MessageBoxResult.No)
            return;

        if (string.IsNullOrEmpty(txtId.Text)) {
            MessageBox.Show("Selecione um registro clicando duas vezes nele!");
            return;
        }

        Programas.Remove(SelectedPrograma);
        CleanFields();
        new ConfigController().Save(Programas);
        dtProgramas.Items.Refresh();
    }

    protected override void OnClosed(EventArgs e)
    {
        MainWindow mainWindow = new MainWindow();
        this.Close();
        mainWindow.Show();
    }
    #endregion

    #region AUXILIARY METHODS
    private void EnableEdition()
    {
        gridDados.IsEnabled = true;
        btnAlterar.IsEnabled = false;
        btnExcluir.IsEnabled = false;
        btnNovo.IsEnabled = false;
        dtProgramas.IsEnabled = false;

        btnOk.IsEnabled = true;
        btnCancelar.IsEnabled = true;
    }

    private void DisableEdition()
    {
        gridDados.IsEnabled = false;
        btnAlterar.IsEnabled = true;
        btnExcluir.IsEnabled = true;
        btnNovo.IsEnabled = true;
        dtProgramas.IsEnabled = true;

        btnOk.IsEnabled = false;
        btnCancelar.IsEnabled = false;
    }

    private void CleanFields()
    {
        txtId.Text = "";
        txtNome.Text = "";
        txtCaminho.Text = "";
        txtArgumentos.Text = "";
        chk32bits.IsChecked = false;
        chk64bits.IsChecked = false;
    }

    void LoadFileds()
    {
        if (SelectedPrograma is null)
            return;

        txtId.Text = SelectedPrograma.Id.ToString();
        txtNome.Text = SelectedPrograma.Nome.ToString();
        txtCaminho.Text = SelectedPrograma.Caminho.ToString();
        txtArgumentos.Text = SelectedPrograma.Argumentos.ToString();
        chk32bits.IsChecked = SelectedPrograma.x86 == "S" ? true : false;
        chk64bits.IsChecked = SelectedPrograma.x64 == "S" ? true : false;
    }

    void SaveFileds()
    {
        SelectedPrograma.Id = Convert.ToInt32(txtId.Text);
        SelectedPrograma.Nome = txtNome.Text;
        SelectedPrograma.Caminho = txtCaminho.Text;
        SelectedPrograma.Argumentos = txtArgumentos.Text;
        SelectedPrograma.x86 = (bool)chk32bits.IsChecked ? "S" : "N";
        SelectedPrograma.x64 = (bool)chk64bits.IsChecked ? "S" : "N";
    }
    #endregion
}
