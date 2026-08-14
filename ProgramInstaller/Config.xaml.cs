using ProgramInstaller.Controllers;
using ProgramInstaller.Models;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProgramInstaller;

public partial class Config : Window
{
    private readonly ConfigController _configController = new();
    private Programas _programas = new();

    public Config()
    {
        InitializeComponent();
        LoadPrograms();
    }

    private void btnNovo_Click(object sender, RoutedEventArgs e)
    {
        ProgramaEditor editor = new() { Owner = this };

        if (editor.ShowDialog() != true)
            return;

        _programas.ListaProgramas.Add(editor.Result);
        SaveAndRefresh(editor.Result);
    }

    private void btnEditarLinha_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: Programa programa })
            EditProgram(programa);
    }

    private void EditProgram(Programa programa)
    {
        ProgramaEditor editor = new(programa) { Owner = this };

        if (editor.ShowDialog() != true)
            return;

        int index = _programas.ListaProgramas.IndexOf(programa);
        _programas.ListaProgramas[index] = editor.Result;
        SaveAndRefresh(editor.Result);
    }

    private void btnExcluirLinha_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: Programa programa })
            return;

        MessageBoxResult dialogResult = MessageBox.Show(
            $"Deseja remover {programa.Nome} da sua lista?",
            "Remover programa",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (dialogResult != MessageBoxResult.Yes)
            return;

        _programas.ListaProgramas.Remove(programa);
        _configController.Save(_programas);
        lstProgramas.Items.Refresh();
        UpdateViewState();
    }

    private void btnAlternarStatus_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: Programa programa })
            return;

        programa.Ativo = !programa.Ativo;
        _configController.Save(_programas);
        UpdateViewState();
    }

    private void LoadPrograms()
    {
        _programas = _configController.Load();
        lstProgramas.ItemsSource = _programas.ListaProgramas;
        UpdateViewState();
    }

    private void SaveAndRefresh(Programa selectedPrograma)
    {
        _configController.Save(_programas);
        lstProgramas.Items.Refresh();
        lstProgramas.ScrollIntoView(selectedPrograma);
        UpdateViewState();
    }

    private void UpdateViewState()
    {
        int active = _programas.ListaProgramas.Count(programa => programa.Ativo);
        int total = _programas.ListaProgramas.Count;

        txtResumo.Text = $"{total} item(ns) · {active} ativo(s)";
        emptyState.Visibility = total == 0 ? Visibility.Visible : Visibility.Collapsed;
    }
}
