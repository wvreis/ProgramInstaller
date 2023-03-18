using ProgramInstaller.Controllers;
using ProgramInstaller.Helpers;
using ProgramInstaller.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace ProgramInstaller;
/// <summary>
/// Interaction logic for Config.xaml
/// </summary>
public partial class Config : Window {
    Programas programas { get; set; } = new();

    public Config()
    {
        InitializeComponent();
        XmlHandler.GetDados(dtProgramas, selectFirstIndex: true);

        //todo: apagar esse teste
        programas.ListaProgramas.Add(new() { 
            Id = 1,
            Nome = "Teste",
            Argumentos = string.Empty,
            Caminho = "winget",
            x64 = "S",
            x86 = "S"
        });

        new ConfigController().Save(programas);

        programas = null;

        programas = new ConfigController().Load();
    }

    private void dtProgramas_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        DataRowView dt = (DataRowView)dtProgramas.SelectedItem;

        if (dt is null)
            return;

        txtId.Text = dt.Row.ItemArray[0].ToString();
        txtNome.Text = dt.Row.ItemArray[1].ToString();
        txtCaminho.Text = dt.Row.ItemArray[2].ToString();
        txtArgumentos.Text = dt.Row.ItemArray[3].ToString();

        if (dt.Row.ItemArray[4].ToString() == "S") {
            chk32bits.IsChecked = true;
        }
        else {
            chk32bits.IsChecked = false;
        }

        if (dt.Row.ItemArray[5].ToString() == "S") {
            chk64bits.IsChecked = true;
        }
        else {
            chk64bits.IsChecked = false;
        }
    }

    private void AlterarDados()
    {
        if (txtId.Text != "") {
            XmlHandler.EditarDados(txtId.Text, txtNome.Text, txtCaminho.Text, txtArgumentos.Text, (bool)chk32bits.IsChecked, (bool)chk64bits.IsChecked);

            XmlHandler.GetDados(dtProgramas, selectFirstIndex: true);

            System.Windows.MessageBox.Show("Dados salvos com sucesso.");
        }
        else {
            System.Windows.MessageBox.Show("Preencha o Campo Id!!!");
        }

    }

    private void AtivarEdicao()
    {
        gridDados.IsEnabled = true;
        btnAlterar.IsEnabled = false;
        btnExcluir.IsEnabled = false;
        btnNovo.IsEnabled = false;
        dtProgramas.IsEnabled = false;

        btnOk.IsEnabled = true;
        btnCancelar.IsEnabled = true;
    }

    private void DesativarEdicao()
    {
        gridDados.IsEnabled = false;
        btnAlterar.IsEnabled = true;
        btnExcluir.IsEnabled = true;
        btnNovo.IsEnabled = true;
        dtProgramas.IsEnabled = true;

        btnOk.IsEnabled = false;
        btnCancelar.IsEnabled = false;
    }

    private void LimparTela()
    {
        txtId.Text = "";
        txtNome.Text = "";
        txtCaminho.Text = "";
        txtArgumentos.Text = "";
        chk32bits.IsChecked = false;
        chk64bits.IsChecked = false;
    }

    private void btnOk_Click(object sender, RoutedEventArgs e)
    {
        if (txtId.Text != "") {
            if (this.Title.Contains("Configuração - Inserindo Novo")) {
                XmlHandler.InserirDados(this);
                LimparTela();
                DesativarEdicao();
                XmlHandler.GetDados(dtProgramas, selectFirstIndex: true);
            }
            else {
                AlterarDados();
                LimparTela();
                DesativarEdicao();
                XmlHandler.GetDados(dtProgramas, selectFirstIndex: true);
            }
        }
        else {
            MessageBox.Show("Preencha o Campo Id!!!");
        }
    }

    private void btnCancelar_Click(object sender, RoutedEventArgs e)
    {
        LimparTela();
        DesativarEdicao();
        this.Title = "Configuração";
    }

    private void btnNovo_Click(object sender, RoutedEventArgs e)
    {
        LimparTela();
        AtivarEdicao();
        this.Title += " - Inserindo Novo";
    }

    private void btnAlterar_Click(object sender, RoutedEventArgs e)
    {
        if (txtId.Text != "") {
            AtivarEdicao();
            this.Title += " - Alterando";
        }
        else {
            System.Windows.MessageBox.Show("Selecione um registro clicando duas vezes nele!");
        }

    }

    private void btnExcluir_Click(object sender, RoutedEventArgs e)
    {

        if (txtId.Text != "") {
            XmlHandler.ExcluirDados(txtId.Text);
            LimparTela();
            XmlHandler.GetDados(dtProgramas, selectFirstIndex: true);
        }
        else {
            System.Windows.MessageBox.Show("Selecione um registro clicando duas vezes nele!");
        }

    }

    protected override void OnClosed(EventArgs e)
    {
        MainWindow mainWindow = new MainWindow();
        this.Close();
        mainWindow.Show();
    }
}
